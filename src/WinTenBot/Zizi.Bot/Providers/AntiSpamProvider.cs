using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Providers
{
    public static class AntiSpamProvider
    {

        [Obsolete("Soon will be replaced by AntiSpamService")]
        public static async Task<SpamWatch> CheckSpamWatch(this int userId)
        {
            var spamWatch = new SpamWatch();
            var spamWatchToken = BotSettings.SpamWatchToken;

            try
            {
                var baseUrl = $"https://api.spamwat.ch/banlist/{userId}";
                spamWatch = await baseUrl
                    .WithOAuthBearerToken(spamWatchToken)
                    .GetJsonAsync<SpamWatch>();
                spamWatch.IsBan = spamWatch.Code != 404;
                Log.Debug("SpamWatch Result: {0}", spamWatch.ToJson(true));
            }
            catch (FlurlHttpException ex)
            {
                var callHttpStatus = ex.Call.HttpResponseMessage.StatusCode;
                // var callHttpStatus = ex.Call.HttpStatus;
                Log.Information("StatusCode: {0}", callHttpStatus);
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

            return spamWatch;
        }

        [Obsolete("Soon will be replaced by AntiSpamService")]
        public static async Task<bool> CheckGBan(this int userId)
        {
            // var query = await new Query("fban_user")
            //     .Where("user_id", userId)
            //     .ExecForSqLite(true)
            //     .GetAsync();

            var jsonGBan = "gban-users".OpenJson();

            Log.Debug("Opening GBan collection");
            var gBanCollection = await jsonGBan.GetCollectionAsync<GlobalBanData>();

            var allBan = gBanCollection.AsQueryable().ToList();
            Log.Debug("Loaded ES2 Ban: {0}", allBan.Count);

            var findBan = allBan.Where(x => x.UserId == userId).ToList();

            var isGBan = findBan.Any();
            Log.Information("UserId {0} is ES2 GBan? {1}", userId, isGBan);

            jsonGBan.Dispose();
            allBan.Clear();

            return isGBan;
        }

        [Obsolete("Soon will be replaced by AntiSpamService")]
        public static async Task<bool> IsCasBanAsync(this User user)
        {
            try
            {
                var userId = user.Id;
                var url = "https://api.cas.chat/check".SetQueryParam("user_id", userId);
                var resp = await url.GetJsonAsync<CasBan>();

                Log.Debug("CasBan Response: {0}", resp.ToJson(true));

                var isBan = resp.Ok;
                Log.Information("UserId: {0} is CAS ban: {1}", userId, isBan);
                return isBan;
            }
            catch
            {
                return false;
            }
        }
    }
}