using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WinTenBot.Common;
using WinTenBot.Enums;
using WinTenBot.Services;

namespace WinTenBot.Handlers.Commands.Welcome
{
    public class WelcomeCommand : CommandBase
    {
        private SettingsService _settingsService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = context.Update.Message;
            _settingsService = new SettingsService(msg);


            Log.Information($"Args: {string.Join(" ", args)}");
            var sendText = "Perintah /welcome hanya untuk grup saja";

            if (msg.Chat.Type == ChatType.Private)
            {
                await _telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
                return;
            }

            var chatTitle = msg.Chat.Title;
            var settings = await _settingsService.GetSettingByGroup()
                .ConfigureAwait(false);
            var welcomeMessage = settings.WelcomeMessage;
            var welcomeButton = settings.WelcomeButton;
            var welcomeMedia = settings.WelcomeMedia;
            var welcomeMediaType = settings.WelcomeMediaType;
            // var splitWelcomeButton = welcomeButton.Split(',').ToList<string>();
            
            sendText = $"⚙ Konfigurasi Welcome di <b>{chatTitle}</b>\n\n";
            if (welcomeMessage.IsNullOrEmpty())
            {
                var defaultWelcome = "Hai {allNewMember}" +
                                     "\nSelamat datang di kontrakan {chatTitle}" +
                                     "\nKamu adalah anggota ke-{memberCount}";
                sendText += "Tidak ada konfigurasi pesan welcome, pesan default akan di terapkan" +
                            $"\n\n<code>{defaultWelcome}</code>" +
                            $"\n\nUntuk bantuan silakan ketik /help" +
                            $"\nBantuan pesan Welcome ke Bantuan > Grup > Welcome";
            }
            else
            {
                sendText += $"<code>{welcomeMessage}</code>";
            }

            // var keyboard = welcomeButton.ToReplyMarkup(2);
            InlineKeyboardMarkup keyboard = null;
            if (!welcomeButton.IsNullOrEmpty())
            {
                keyboard = welcomeButton.ToReplyMarkup(2);

                sendText += "\n\n<b>Raw Button:</b>" +
                            $"\n<code>{welcomeButton}</code>";
            }
            
            if (welcomeMediaType != MediaType.Unknown)
            {
                await _telegramService.SendMediaAsync(welcomeMedia, welcomeMediaType, sendText, keyboard)
                    .ConfigureAwait(false);
            }
            else
            {
                await _telegramService.SendTextAsync(sendText, keyboard)
                    .ConfigureAwait(false);
            }
        }
    }
}