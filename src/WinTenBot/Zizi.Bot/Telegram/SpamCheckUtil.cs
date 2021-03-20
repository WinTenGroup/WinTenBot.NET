using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class SpamCheckUtil
    {
        [Obsolete("Soon will be replaced by AntiSpamService")]
        public static async Task<bool> CheckGlobalBanAsync(this TelegramService telegramService,
            User userTarget = null)
        {
            var message = telegramService.MessageOrEdited;
            var sw = Stopwatch.StartNew();
            var user = message.From;

            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedEs2)
            {
                Log.Information("Fed ES2 Ban is disabled in this Group!");
                return false;
            }

            if (!telegramService.IsBotAdmin)
            {
                Log.Information("This bot IsNot Admin in {0}, so ES2 check disabled. Time: {1}", message.Chat.Id,
                    sw.Elapsed);
                sw.Stop();

                return false;
            }

            if (telegramService.IsFromAdmin)
            {
                Log.Information("This UserID {0} Is Admin in {1}, so ES2 check disabled. Time: {2}", user.Id,
                    message.Chat.Id, sw.Elapsed);
                sw.Stop();

                return false;
            }

            Log.Information("Starting check Global Ban");

            if (userTarget != null) user = userTarget;

            var messageId = message.MessageId;

            var isBan = await user.Id.CheckGBan();
            Log.Information("IsBan: {IsBan}", isBan);
            if (isBan)
            {
                await telegramService.DeleteAsync(messageId);
                await telegramService.KickMemberAsync(user);
                await telegramService.UnbanMemberAsync(user);
            }

            sw.Stop();

            return isBan;
        }

        [Obsolete("Soon will be replaced by AntiSpamService")]
        public static async Task<bool> CheckCasBanAsync(this TelegramService telegramService)
        {
            bool isBan;
            var message = telegramService.MessageOrEdited;
            var sw = Stopwatch.StartNew();
            var user = message.From;
            var userId = user.Id;

            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedCasBan)
            {
                Log.Information("Fed CAS Ban is disabled in this Group!");
                return false;
            }

            if (!telegramService.IsBotAdmin)
            {
                Log.Information("This bot IsNot Admin in {0}, CAS disabled. Time: {1}", message.Chat.Id, sw.Elapsed);
                sw.Stop();

                return false;
            }

            if (telegramService.IsFromAdmin)
            {
                Log.Information("This UserID {0} Is Admin in {1}, CAS disabled. Time: {2}", user.Id, message.Chat.Id, sw.Elapsed);
                sw.Stop();

                return false;
            }

            Log.Information("Starting check in Cas Ban");

            isBan = await user.IsCasBanAsync();
            Log.Information("{User} is CAS ban: {IsBan}", user, isBan);
            if (isBan)
            {
                var replyMarkup = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("CAS Query", $"https://cas.chat/query?u={userId}"),
                        InlineKeyboardButton.WithUrl("CAS Discuss", "https://t.me/cas_discussion")
                    }
                });

                var sendText = $"{user} di blokir di CAS!" +
                               $"\nUntuk detil lebih lanjut, silakan buka <b>CAS Query</b>" +
                               $"\n\nUntuk mengajukan buka blokir silakan masuk ke <b>CAS Discuss</b>.";
                var isAdminGroup = await telegramService.IsAdminGroup();
                if (!isAdminGroup)
                {
                    await telegramService.KickMemberAsync(user);

                    await telegramService.UnbanMemberAsync(user);
                }
                else
                {
                    sendText = $"{user} di blokir di CAS, namun tidak bisa memblokirnya karena Admin di Grup ini";
                }

                await telegramService.SendTextAsync(sendText, replyMarkup);
            }

            sw.Stop();

            return isBan;
        }

        [Obsolete("Soon will be replaced by AntiSpamService")]
        public static async Task<bool> CheckSpamWatchAsync(this TelegramService telegramService)
        {
            bool isBan;

            var message = telegramService.MessageOrEdited;
            var sw = Stopwatch.StartNew();
            var user = message.From;

            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedSpamWatch)
            {
                Log.Information("Fed SpamWatch is disabled in this Group!");
                return false;
            }

            if (!telegramService.IsBotAdmin)
            {
                Log.Information("This bot IsNot Admin in {0}, so SpamWatch check disabled. Time: {1}", message.Chat.Id,
                    sw.Elapsed);
                sw.Stop();

                return false;
            }

            if (telegramService.IsFromAdmin)
            {
                Log.Information("This UserID {0} Is Admin in {1}, so SpamWatch check disabled. Time: {2}", user.Id,
                    message.Chat.Id, sw.Elapsed);
                sw.Stop();

                return false;
            }

            Log.Information("Starting Run SpamWatch");

            var spamWatch = await user.Id.CheckSpamWatch();
            isBan = spamWatch.IsBan;

            Log.Information("{0} is SpamWatch Ban => {1}", user, isBan);

            if (isBan)
            {
                var sendText = $"{user} is banned in SpamWatch!" +
                               "\nFed: @SpamWatch" +
                               $"\nReason: {spamWatch.Reason}";
                await telegramService.SendTextAsync(sendText);
                await telegramService.KickMemberAsync(user);
                await telegramService.UnbanMemberAsync(user);
            }

            sw.Stop();

            return isBan;
        }
    }
}