using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using EasyCaching.Core;
using Flurl;
using Flurl.Http;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Services.Datas;

namespace Zizi.Bot.Services.Features
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
        public async Task<string> CheckSpam(int userId)
        {
            var spamWatchTask = CheckSpamWatch(userId);
            var casBanTask = IsCasBanAsync(userId);
            var gBanTask = CheckEs2Ban(userId);

            await Task.WhenAll(spamWatchTask, casBanTask, gBanTask);

            var swBan = spamWatchTask.Result;
            var casBan = casBanTask.Result;
            var es2Ban = gBanTask.Result;

            if (!(swBan || casBan || es2Ban))
            {
                Log.Information("UserId {0} is passed on all Fed Ban", userId);
                return string.Empty;
            }

            var banMsg = $"Pengguna {userId} telah di Ban di Federasi";

            if (es2Ban) banMsg += "\n- ES2 Global Ban";
            if (casBan) banMsg += "\n- CAS Fed";
            if (swBan) banMsg += "\n- SpamWatch Fed";

            return banMsg;
        }

        /// <summary>
        /// Checks the es2 ban.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A Task.</returns>
        public async Task<bool> CheckEs2Ban(int userId)
        {
            var sw = Stopwatch.StartNew();

            var user = await _globalBanService.GetGlobalBanByIdC(userId);
            var isBan = user != null;

            Log.Debug("UserId: {0} is CAS ban: {1}", userId, isBan);

            Log.Debug("ES2 Check complete in {0}", sw.Elapsed);
            sw.Stop();

            return isBan;
        }

        /// <summary>
        /// Checks the spam watch.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A Task.</returns>
        public async Task<bool> CheckSpamWatch(int userId)
        {
            var sw = Stopwatch.StartNew();

            var spamWatch = new SpamWatch();
            var spamWatchToken = _commonConfig.SpamWatchToken;

            try
            {
                var baseUrl = $"https://api.spamwat.ch/banlist/{userId}";

                if (!await _cachingProvider.ExistsAsync(baseUrl))
                {
                    var check = await baseUrl
                        .WithOAuthBearerToken(spamWatchToken)
                        .GetJsonAsync<SpamWatch>();

                    await _cachingProvider.SetAsync(baseUrl, check, TimeSpan.FromMinutes(5));
                }

                var cache = await _cachingProvider.GetAsync<SpamWatch>(baseUrl);

                spamWatch = cache.Value;

                spamWatch.IsBan = spamWatch.Code != 404;
                Log.Debug("SpamWatch Result: {0}", spamWatch.ToJson(true));
            }
            catch (FlurlHttpException ex)
            {
                var callHttpStatus = ex.Call.HttpResponseMessage.StatusCode;
                // var callHttpStatus = ex.Call.HttpStatus;
                Log.Debug("StatusCode: {0}", callHttpStatus);
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

            Log.Debug("UserId: {0} is CAS ban: {1}", userId, isBan);

            Log.Debug("Spamwatch Check complete in {0}", sw.Elapsed);
            sw.Stop();

            return isBan;
        }

        /// <summary>
        /// Are the cas ban async.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A Task.</returns>
        public async Task<bool> IsCasBanAsync(int userId)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                var url = "https://api.cas.chat/check".SetQueryParam("user_id", userId);
                var resp = await url.GetJsonAsync<CasBan>();

                Log.Debug("CasBan Response: {0}", resp.ToJson(true));

                var isBan = resp.Ok;
                Log.Debug("UserId: {0} is CAS ban: {1}", userId, isBan);

                Log.Debug("CAS Check complete in {0}", sw.Elapsed);
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