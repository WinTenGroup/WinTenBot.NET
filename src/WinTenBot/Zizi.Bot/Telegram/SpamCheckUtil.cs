using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class SpamCheckUtil
    {
        public static async Task<bool> CheckGlobalBanAsync(this TelegramService telegramService,
            User userTarget = null)
        {
            Log.Information("Starting check Global Ban");

            var message = telegramService.MessageOrEdited;
            var user = message.From;

            // var settingService = new SettingsService(message);
            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedEs2)
            {
                Log.Information("Fed ES2 Ban is disabled in this Group!");
                return false;
            }

            var isBotAdmin = await telegramService.IsBotAdmin().ConfigureAwait(false);
            if (!isBotAdmin)
            {
                Log.Information("This bot IsNot Admin in {0}, so ES2 check disabled.", message.Chat);
                return false;
            }

            var isFromAdmin = await telegramService.IsAdminGroup().ConfigureAwait(false);
            if (isFromAdmin)
            {
                Log.Information("This UserID {0} Is Admin in {1}, so ES2 check disabled.", user.Id, message.Chat.Id);
                return false;
            }

            if (userTarget != null) user = userTarget;

            var messageId = message.MessageId;

            var isBan = await user.Id.CheckGBan()
                .ConfigureAwait(false);
            Log.Information($"IsBan: {isBan}");
            if (isBan)
            {
                await telegramService.DeleteAsync(messageId)
                    .ConfigureAwait(false);
                await telegramService.KickMemberAsync(user)
                    .ConfigureAwait(false);
                await telegramService.UnbanMemberAsync(user)
                    .ConfigureAwait(false);
            }

            return isBan;
        }

        public static async Task<bool> CheckCasBanAsync(this TelegramService telegramService)
        {
            bool isBan;
            Log.Information("Starting check in Cas Ban");
            var message = telegramService.MessageOrEdited;
            var user = message.From;
            var userId = user.Id;

            // var settingService = new SettingsService(message);
            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedCasBan)
            {
                Log.Information("Fed Cas Ban is disabled in this Group!");
                return false;
            }

            var isBotAdmin = await telegramService.IsBotAdmin().ConfigureAwait(false);
            if (!isBotAdmin)
            {
                Log.Information("This bot IsNot Admin in {0}, so CAS check disabled.", message.Chat);
                return false;
            }

            var isFromAdmin = await telegramService.IsAdminGroup().ConfigureAwait(false);
            if (isFromAdmin)
            {
                Log.Information("This UserID {0} Is Admin in {1}, so CAS check disabled.", user.Id, message.Chat.Id);
                return false;
            }

            isBan = await user.IsCasBanAsync()
                .ConfigureAwait(false);
            Log.Information($"{user} is CAS ban: {isBan}");
            if (isBan)
            {
                var sendText = $"{user} di blokir di CAS!" +
                               $"\nUntuk detil lebih lanjut, silakan kunjungi alamat berikut." +
                               $"\nhttps://cas.chat/query?u={userId}" +
                               $"\n\nUntuk verifikasi bahwa ini kesalahan silakan kunjungi alamat berikut." +
                               $"\nhttps://t.me/cas_discussion";
                var isAdminGroup = await telegramService.IsAdminGroup().ConfigureAwait(false);
                if (!isAdminGroup)
                {
                    await telegramService.KickMemberAsync(user)
                        .ConfigureAwait(false);

                    await telegramService.UnbanMemberAsync(user)
                        .ConfigureAwait(false);
                }
                else
                {
                    sendText = $"{user} di blokir di CAS, namun tidak bisa memblokirnya karena Admin di Grup ini";
                }

                await telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
            }

            return isBan;
        }

        public static async Task<bool> CheckSpamWatchAsync(this TelegramService telegramService)
        {
            bool isBan;
            Log.Information("Starting Run SpamWatch");

            var message = telegramService.MessageOrEdited;
            var user = message.From;
            // var settingService = new SettingsService(message);
            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedSpamWatch)
            {
                Log.Information("Fed SpamWatch is disabled in this Group!");
                return false;
            }

            var isBotAdmin = await telegramService.IsBotAdmin().ConfigureAwait(false);
            if (!isBotAdmin)
            {
                Log.Information("This bot IsNot Admin in {0}, so SpamWatch check disabled.", message.Chat);
                return false;
            }

            var isFromAdmin = await telegramService.IsAdminGroup().ConfigureAwait(false);
            if (isFromAdmin)
            {
                Log.Information("This UserID {0} Is Admin in {1}, so SpamWatch check disabled.", user.Id, message.Chat.Id);
                return false;
            }
            
            var spamWatch = await user.Id.CheckSpamWatch()
                .ConfigureAwait(false);
            isBan = spamWatch.IsBan;

            Log.Information("{0} is SpamWatch Ban => {1}", user, isBan);

            if (isBan)
            {
                var sendText = $"{user} is banned in SpamWatch!" +
                               "\nFed: @SpamWatch" +
                               $"\nReason: {spamWatch.Reason}";
                await telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
                await telegramService.KickMemberAsync(user)
                    .ConfigureAwait(false);
                await telegramService.UnbanMemberAsync(user)
                    .ConfigureAwait(false);
            }

            return isBan;
        }
    }
}