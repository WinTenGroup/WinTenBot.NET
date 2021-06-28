using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using EasyCaching.Core;
using Flurl;
using Flurl.Http;
using Serilog;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Services
{
    /// <summary>
    /// The anti spam service.
    /// </summary>
    public class AntiSpamService
    {
        private readonly CommonConfig _commonConfig;
        private readonly GlobalBanService _globalBanService;
        private readonly IEasyCachingProvider _cachingProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiSpamService"/> class.
        /// </summary>
        /// <param name="commonConfig">The common config.</param>
        /// <param name="cachingProvider"></param>
        /// <param name="globalBanService">The global ban service.</param>
        public AntiSpamService(
            CommonConfig commonConfig,
            IEasyCachingProvider cachingProvider,
            GlobalBanService globalBanService
        )
        {
            _commonConfig = commonConfig;
            _cachingProvider = cachingProvider;
            _globalBanService = globalBanService;
        }

        /// <summary>
        /// Checks the spam.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A Task.</returns>
        public async Task<AntiSpamResult> CheckSpam(long userId)
        {
            var spamWatchTask = CheckSpamWatch(userId);
            var casBanTask = IsCasBanAsync(userId);
            var gBanTask = CheckEs2Ban(userId);

            await Task.WhenAll(spamWatchTask, casBanTask, gBanTask);

            var swBan = spamWatchTask.Result;
            var casBan = casBanTask.Result;
            var es2Ban = gBanTask.Result;
            var anyBan = swBan || casBan || es2Ban;

            if (!anyBan)
            {
                Log.Information("UserId {UserId} is passed on all Fed Ban", userId);
                return null;
            }

            var banMsg = $"Pengguna {userId} telah di Ban di Federasi";

            if (es2Ban) banMsg += "\n- ES2 Global Ban";
            if (casBan) banMsg += "\n- CAS Fed";
            if (swBan) banMsg += "\n- SpamWatch Fed";

            // return banMsg;

            return new AntiSpamResult()
            {
                MessageResult = banMsg,
                IsAnyBanned = anyBan,
                IsEs2Banned = es2Ban,
                IsCasBanned = casBan,
                IsSpamWatched = swBan
            };
        }

        /// <summary>
        /// Checks the es2 ban.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A Task.</returns>
        public async Task<bool> CheckEs2Ban(long userId)
        {
            var sw = Stopwatch.StartNew();

            var user = await _globalBanService.GetGlobalBanByIdC(userId);
            var isBan = user != null;

            Log.Debug("UserId: {UserId} is CAS ban: {IsBan}", userId, isBan);

            Log.Debug("ES2 Check complete in {Elapsed}", sw.Elapsed);
            sw.Stop();

            return isBan;
        }

        /// <summary>
        /// Checks the spam watch.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A Task.</returns>
        public async Task<bool> CheckSpamWatch(long userId)
        {
            var sw = Stopwatch.StartNew();
            var cacheKey = $"sw-ban_{userId}";


            var spamWatch = new SpamWatchResult();
            var spamWatchToken = _commonConfig.SpamWatchToken;

            try
            {
                var baseUrl = $"https://api.spamwat.ch/banlist/{userId}";

                if (!await _cachingProvider.ExistsAsync(cacheKey))
                {
                    var check = await baseUrl
                        .WithOAuthBearerToken(spamWatchToken)
                        .GetJsonAsync<SpamWatchResult>();

                    await _cachingProvider.SetAsync(baseUrl, check, TimeSpan.FromMinutes(10));
                }

                var cache = await _cachingProvider.GetAsync<SpamWatchResult>(cacheKey);

                spamWatch = cache.Value;

                spamWatch.IsBan = spamWatch.Code != 404;
                Log.Debug("SpamWatch Result: {V}", spamWatch.ToJson(true));
            }
            catch (FlurlHttpException ex)
            {
                var callHttpStatus = ex.Call.HttpResponseMessage.StatusCode;

                Log.Debug("StatusCode: {CallHttpStatus}", callHttpStatus);
                switch (callHttpStatus)
                {
                    case HttpStatusCode.NotFound:
                        spamWatch.IsBan = false;
                        break;

                    case HttpStatusCode.Unauthorized:
                        Log.Warning("Please check your SpamWatch API Token!");
                        Log.Error(ex, "SpamWatch API FlurlHttpEx");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SpamWatch Exception");
            }

            var isBan = spamWatch.IsBan;

            Log.Debug("UserId: {UserId} is CAS ban: {IsBan}", userId, isBan);

            Log.Debug("Spamwatch Check complete in {Elapsed}", sw.Elapsed);
            sw.Stop();

            return isBan;
        }

        /// <summary>
        /// Are the cas ban async.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A Task.</returns>
        public async Task<bool> IsCasBanAsync(long userId)
        {
            var casCacheKey = $"cas-ban_{userId}";
            try
            {
                var sw = Stopwatch.StartNew();

                if (!await _cachingProvider.ExistsAsync(casCacheKey))
                {
                    var url = "https://api.cas.chat/check".SetQueryParam("user_id", userId);
                    var resp = await url.GetJsonAsync<CasBan>();

                    await _cachingProvider.SetAsync(casCacheKey, resp, TimeSpan.FromMinutes(10));
                }

                var cache = await _cachingProvider.GetAsync<CasBan>(casCacheKey);
                Log.Debug("CasBan Result: {V}", cache.ToJson(true));

                var isBan = cache.Value.Ok;
                Log.Debug("UserId: {UserId} is CAS ban: {IsBan}", userId, isBan);

                Log.Debug("CAS Check complete in {Elapsed}", sw.Elapsed);
                sw.Stop();

                return isBan;
            }
            catch
            {
                return false;
            }
        }
    }
}