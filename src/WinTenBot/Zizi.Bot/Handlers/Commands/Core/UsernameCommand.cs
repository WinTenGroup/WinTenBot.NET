using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class UsernameCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            if (!await _telegramService.IsBeta().ConfigureAwait(false)) return;

            var urlStart = await _telegramService.GetUrlStart("start=set-username")
                .ConfigureAwait(false);
            var usernameStr = _telegramService.IsNoUsername ? "belum" : "sudah";
            var sendText = "Tentang Username" +
                           $"\nKamu {usernameStr} mengatur Username";

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("Cara Pasang Username", urlStart)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Verifikasi Username", "verify username-only")
                }
            });

            await _telegramService.SendTextAsync(sendText, inlineKeyboard)
                .ConfigureAwait(false);
        }
    }
}