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

        public static bool IsRestricted()
        {
            return BotSettings.GlobalConfiguration["CommonConfig:IsRestricted"].ToBool();
        }

        public static bool CheckRestriction(this long chatId)
        {
            try
            {
                var isRestricted = false;
                var globalRestrict = IsRestricted();
                var sudoers = BotSettings.GlobalConfiguration.GetSection("RestrictArea").Get<List<string>>();
                var match = sudoers.FirstOrDefault(x => x == chatId.ToString());

                Log.Information($@"Global Restriction: {globalRestrict}");
                if (match == null && globalRestrict)
                {
                    isRestricted = true;
                }

                Log.Information($"ChatId: {chatId} IsRestricted: {isRestricted}");
                return isRestricted;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Chat Restriction.");
                return false;
            }
        }

        public static async Task<bool> EnsureChatRestrictionAsync(this TelegramService telegramService)
        {
            Log.Information("Starting ensure Chat Restriction");

            var message = telegramService.MessageOrEdited;
            var chatId = message.Chat.Id;

            if (!chatId.CheckRestriction()) return false;

            Log.Information("I must leave right now!");
            var msgOut = $"Sepertinya saya salah alamat, saya pamit dulu..";

            await telegramService.SendTextAsync(msgOut)
                .ConfigureAwait(false);
            await telegramService.LeaveChat(chatId)
                .ConfigureAwait(false);
            return true;
        }

        public static ITelegramBotClient GetClient(string name)
        {
            return BotSettings.Clients[name];
        }

        public static async Task EnsureChatHealthAsync(this TelegramService telegramService)
        {
            Log.Information("Ensuring chat health..");

            var message = telegramService.Message;
            var settingsService = new SettingsService
            {
                Message = message
            };

            var data = new Dictionary<string, object>()
            {
                ["chat_id"] = message.Chat.Id,
                ["chat_title"] = message.Chat.Title,
            };

            var update = await settingsService.SaveSettingsAsync(data)
                .ConfigureAwait(false);

            await settingsService.UpdateCache()
                .ConfigureAwait(false);
        }
    }
}