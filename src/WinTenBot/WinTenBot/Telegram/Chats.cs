using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using WinTenBot.Common;
using WinTenBot.Model;
using WinTenBot.Services;

namespace WinTenBot.Telegram
{
    public static class Chats
    {
        public static long ReduceChatId(this long chatId)
        {
            var chatIdStr = chatId.ToString();
            if (chatIdStr.StartsWith("-100"))
            {
                chatIdStr = chatIdStr.Substring(4);
            }

            $"Reduced ChatId from {chatId} to {chatIdStr}".LogDebug();

            return chatIdStr.ToInt64();
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
                ["chat_type"] = message.Chat.Type
            };

            var update = await settingsService.SaveSettingsAsync(data)
                .ConfigureAwait(false);

            await settingsService.UpdateCache()
                .ConfigureAwait(false);
        }

        #region Chat Restriction

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

        #endregion
    }
}