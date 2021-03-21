using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class AboutCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly EnginesConfig _enginesConfig;

        public AboutCommand(TelegramService telegramService, EnginesConfig enginesConfig)
        {
            _telegramService = telegramService;
            _enginesConfig = enginesConfig;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var me = await _telegramService.GetMeAsync();
            var botName = me.FirstName;
            var botVersion = _enginesConfig.Version;

            var sendText = $"<b>{botName} (.NET)";

            if (await _telegramService.IsBeta())
            {
                sendText += " Alpha Preview</b>";
            }


            sendText += $"\nby @WinTenDev" +
                        $"\nVersion: {botVersion}" +
                        "\n\nℹ️ Bot Telegram resmi berbasis <b>WinTen API.</b> untuk manajemen dan peralatan grup. " +
                        "Untuk detail fitur pada perintah /start.\n";

            if (await _telegramService.IsBeta())
            {
                sendText += "\n<b>Saya masih Beta, mungkin terdapat bug dan tidak stabil. " +
                            "Tidak di rekomendasikan untuk grup Anda.</b>\n";
            }

            sendText += "\nUntuk Bot lebih cepat dan tetap cepat dan terus peningkatan dan keandalan, " +
                        "silakan <b>Donasi</b> untuk biaya Server dan beri saya Kopi.\n\n" +
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
                    InlineKeyboardButton.WithUrl("💰 Donate", "http://paypal.me/Azhe403"),
                    InlineKeyboardButton.WithUrl("💰 Dana.ID", "https://link.dana.id/qr/5xcp0ma"),
                    InlineKeyboardButton.WithUrl("💰 Saweria", "https://saweria.co/azhe403")
                }
            });

            await _telegramService.SendTextAsync(sendText, inlineKeyboard);
        }
    }
}