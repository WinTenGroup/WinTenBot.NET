using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Models;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class AboutCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var me = await _telegramService.GetMeAsync()
                .ConfigureAwait(false);
            var botName = me.FirstName;
            var botVersion = BotSettings.ProductVersion;

            var sendText = $"<b>{botName} (.NET) Alpha Preview</b>\n" +
                           $"by @WinTenDev\n" +
                           $"Version: {botVersion}\n\n" +
                           "ℹ️ Bot Telegram resmi berbasis <b>WinTen API.</b> untuk manajemen dan peralatan grup. " +
                           "Untuk detail fitur pada perintah /start.\n\n";

            if (await _telegramService.IsBeta()
                .ConfigureAwait(false))
            {
                sendText += "<b>Saya masih Beta, mungkin terdapat bug dan tidak stabil. " +
                            "Tidak di rekomendasikan untuk grup Anda.</b>\n\n";
            }

            sendText += "Untuk Bot lebih cepat dan tetap cepat dan terus peningkatan dan keandalan, " +
                        "silakan <b>Donasi</b> via Paypal untuk beli VPS dan beri saya Kopi.\n\n" +
                        "Terima kasih kepada <b>Akmal Projext</b> yang telah memberikan kesempatan ZiziBot pada kehidupan sebelumnya.";

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("👥 WinTen Group", "https://t.me/WinTenGroup"),
                    InlineKeyboardButton.WithUrl("❤️ WinTen Dev", "https://t.me/WinTenDev")
                },
                new[]
                {
                    InlineKeyboardButton.WithUrl("👥 Redmi 5A (ID)", "https://t.me/Redmi5AID"),
                    InlineKeyboardButton.WithUrl("👥 Telegram Bot API", "https://t.me/TgBotID")
                },
                new[]
                {
                    InlineKeyboardButton.WithUrl("💽 Source Code (.NET)", "https://github.com/WinTenDev/WinTenBot.NET"),
                    InlineKeyboardButton.WithUrl("🏗 Akmal Projext", "https://t.me/AkmalProjext")
                },
                new[]
                {
                    InlineKeyboardButton.WithUrl("💰 Donate", "http://paypal.me/Azhe403")
                }
            });

            await _telegramService.SendTextAsync(sendText, inlineKeyboard)
                .ConfigureAwait(false);
        }
    }
}