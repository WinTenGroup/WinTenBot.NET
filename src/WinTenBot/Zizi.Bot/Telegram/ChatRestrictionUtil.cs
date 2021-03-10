using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class ChatRestrictionUtil
    {
        [Obsolete("This method will be moved to TelegramService")]
        public static bool IsRestricted()
        {
            var isRestricted = BotSettings.GlobalConfiguration["CommonConfig:IsRestricted"].ToBool();
            Log.Debug("Global Restriction: {0}", isRestricted);

            return isRestricted;
        }

        [Obsolete("This method will be moved to TelegramService")]
        public static bool CheckRestriction(this long chatId)
        {
            try
            {
                var isRestricted = false;
                var sudoers = BotSettings.GlobalConfiguration.GetSection("RestrictArea").Get<List<string>>();
                var match = sudoers.FirstOrDefault(x => x == chatId.ToString());

                if (match == null)
                {
                    isRestricted = true;
                }

                Log.Information("ChatId: {0} IsRestricted: {1}", chatId, isRestricted);
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

            var globalRestrict = ChatRestrictionUtil.IsRestricted();
            var isRestricted = chatId.CheckRestriction();

            if (!isRestricted || !globalRestrict) return false;

            Log.Information("I must leave right now!");
            var msgOut = $"Sepertinya saya salah alamat, saya pamit dulu..";

            await telegramService.SendTextAsync(msgOut)
                .ConfigureAwait(false);
            await telegramService.LeaveChat(chatId)
                .ConfigureAwait(false);
            return true;
        }

    }
}