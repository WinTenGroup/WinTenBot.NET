using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WinTenBot.Common;
using WinTenBot.Model;
using WinTenBot.Services;

namespace WinTenBot.Telegram
{
    public static class Health
    {
        public static async Task<string> GetUrlStart(this TelegramService telegramService, string param)
        {
            var bot = await telegramService.Client.GetMeAsync()
                .ConfigureAwait(false);
            var username = bot.Username;
            return $"https://t.me/{username}?{param}";
        }

        public static async Task<User> GetMeAsync(this TelegramService telegramService)
        {
            return await telegramService.Client.GetMeAsync()
                .ConfigureAwait(false);
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
            "Checking is added me?".LogInfo();
            var me = await BotSettings.Client.GetMeAsync()
                .ConfigureAwait(false);
            var isMe = (from user in users where user.Id == me.Id select user.Id == me.Id).FirstOrDefault();
            $"Is added me? {isMe}".LogInfo();

            return isMe;
        }

        public static ITelegramBotClient GetClient(string name)
        {
            return BotSettings.Clients[name];
        }
    }
}