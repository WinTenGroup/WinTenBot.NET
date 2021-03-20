using System;
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

            var me = await telegramService.GetBotMeAsync();
            var isBotAdmin = await telegramService.IsAdminChat(me.Id);
            Log.Debug("Is {0} Admin on Chat {1}? {2}", me.Username, chat.Id, isBotAdmin);

            return isBotAdmin;
        }

        public static async Task<string> GetUrlStart(this TelegramService telegramService, string param)
        {
            // var bot = await telegramService.Client.GetBotMeAsync()
            //     ;
            //
            // Log.Debug("Getting Bot Username");
            // var username = bot.Username;
            // return $"https://t.me/{username}?{param}";

            return await telegramService.Client.GetUrlStart(param);
        }

        public static async Task<string> GetUrlStart(this ITelegramBotClient botClient, string param)
        {
            var getMe = await botClient.GetBotMeAsync();
            var username = getMe.Username;
            return $"https://t.me/{username}?{param}";
        }

        [Obsolete("Please use 'GetBotMeAsync', include cached request")]
        public static async Task<User> GetMeAsync(this TelegramService telegramService)
        {
            return await telegramService.Client.GetMeAsync();
        }

        [Obsolete("This method will be moved to TelegramService")]
        public static async Task<User> GetBotMeAsync(this TelegramService telegramService)
        {
            return await telegramService.Client.GetBotMeAsync();
        }

        [Obsolete("This method will be moved to TelegramService")]
        public static async Task<User> GetBotMeAsync(this ITelegramBotClient botClient)
        {
            Log.Debug("Getting Me");

            var isCacheExist = MonkeyCacheUtil.IsCacheExist(GetMeCacheKey);
            if (!isCacheExist)
            {
                Log.Debug("Request GetMe API");
                var getMe = await botClient.GetMeAsync()
                    ;

                Log.Debug("Updating cache");
                getMe.AddCache(GetMeCacheKey, 10);
            }

            var fromCache = MonkeyCacheUtil.Get<User>(GetMeCacheKey);
            return fromCache;
        }

        [Obsolete("Please use 'GetBotMeAsync', include cached request")]
        public static async Task<bool> IsBeta(this TelegramService telegramService)
        {
            var me = await GetBotMeAsync(telegramService);
            var isBeta = me.Username.ToLower().Contains("beta");
            Log.Information("Is Bot {0} IsBeta: {1}", me, isBeta);
            return isBeta;
        }

        public static async Task<bool> IsBotAdded(this User[] users)
        {
            Log.Information("Checking is added me?");
            var me = await BotSettings.Client.GetBotMeAsync()
                ;
            var isMe = (from user in users where user.Id == me.Id select user.Id == me.Id).FirstOrDefault();
            Log.Information("Is added me? {0}", isMe);

            return isMe;
        }

        [Obsolete("This method will be moved to TelegramService")]
        public static async Task<bool> IsBotAdded(this TelegramService telegramService, User[] users)
        {
            Log.Information("Checking is added me?");

            // var cacheKey = "get-me";
            //
            // var isValid = MonkeyCacheUtil.IsCacheExist(cacheKey);
            // if (!isValid)
            // {
            //     getMe.AddCache(cacheKey);
            // }

            var me = await telegramService.GetBotMeAsync();

            // var me = MonkeyCacheUtil.Get<User>(cacheKey);

            var isMe = (from user in users where user.Id == me.Id select user.Id == me.Id).FirstOrDefault();
            Log.Information("Is added me? {0}", isMe);

            return isMe;
        }

        public static ITelegramBotClient GetClient(string name)
        {
            return BotSettings.Clients[name];
        }
    }
}