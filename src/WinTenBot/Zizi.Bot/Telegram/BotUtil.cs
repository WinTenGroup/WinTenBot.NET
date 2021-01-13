using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Zizi.Bot.Models;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Telegram
{
    public static class BotUtil
    {
        private const string GetMeCacheKey = "getme";

        public static async Task<bool> IsBotAdmin(this TelegramService telegramService)
        {
            var message = telegramService.MessageOrEdited;
            var chat = message.Chat;

            var me = await telegramService.GetMeAsync().ConfigureAwait(false);
            var isBotAdmin = await telegramService.IsAdminChat(me.Id).ConfigureAwait(false);
            Log.Debug("Is {0} Admin on Chat {1}? {2}", me.Username, chat.Id, isBotAdmin);

            return isBotAdmin;
        }

        public static async Task<string> GetUrlStart(this TelegramService telegramService, string param)
        {
            Log.Debug("Getting Me");
            var bot = await telegramService.Client.GetMeAsync()
                .ConfigureAwait(false);

            bot.AddCache("getme");
            Log.Debug("Getting Bot Username");
            var username = bot.Username;
            return $"https://t.me/{username}?{param}";
        }

        public static async Task<string> GetUrlStart(this ITelegramBotClient botClient, string param)
        {
            var getMe = await botClient.GetMe();

            var username = getMe.Username;
            return $"https://t.me/{username}?{param}";
        }

        public static async Task<User> GetMeAsync(this TelegramService telegramService)
        {
            return await telegramService.Client.GetMeAsync()
                .ConfigureAwait(false);
        }

        public static async Task<User> GetMe(this ITelegramBotClient botClient)
        {
            Log.Debug("Request GetMe");

            var isCacheExist = MonkeyCacheUtil.IsCacheExist(GetMeCacheKey);
            if (!isCacheExist)
            {
                Log.Debug("Request GetMe API");
                var getMe = await botClient.GetMeAsync()
                    .ConfigureAwait(false);

                Log.Debug("Updating cache");
                getMe.AddCache(GetMeCacheKey);
            }

            var fromCache = MonkeyCacheUtil.Get<User>(GetMeCacheKey);
            return fromCache;
        }

        public static async Task<bool> IsBeta(this TelegramService telegramService)
        {
            var me = await GetMeAsync(telegramService)
                .ConfigureAwait(false);
            var isBeta = me.Username.ToLower().Contains("beta");
            Log.Information($"IsBeta: {isBeta}");
            return isBeta;
        }

        public static async Task<bool> IsBotAdded(this User[] users)
        {
            Log.Information("Checking is added me?");
            var me = await BotSettings.Client.GetMeAsync()
                .ConfigureAwait(false);
            var isMe = (from user in users where user.Id == me.Id select user.Id == me.Id).FirstOrDefault();
            Log.Information($"Is added me? {isMe}");

            return isMe;
        }

        public static async Task<bool> IsBotAdded(this TelegramService telegramService, User[] users)
        {
            Log.Information("Checking is added me?");

            var cacheKey = "get-me";
            var client = telegramService.Client;

            var isValid = MonkeyCacheUtil.IsCacheExist(cacheKey);
            if (!isValid)
            {
                var getMe = await client.GetMeAsync().ConfigureAwait(false);
                getMe.AddCache(cacheKey);
            }

            var me = MonkeyCacheUtil.Get<User>(cacheKey);


            var isMe = (from user in users where user.Id == me.Id select user.Id == me.Id).FirstOrDefault();
            Log.Information($"Is added me? {isMe}");

            return isMe;
        }

        public static ITelegramBotClient GetClient(string name)
        {
            return BotSettings.Clients[name];
        }
    }
}