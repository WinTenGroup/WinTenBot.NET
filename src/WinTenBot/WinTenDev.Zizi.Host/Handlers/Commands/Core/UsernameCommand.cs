using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Core
{
    public class UsernameCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public UsernameCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            if (!await _telegramService.IsBeta()) return;

            var urlStart = await _telegramService.GetUrlStart("start=set-username");
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

            await _telegramService.SendTextAsync(sendText, inlineKeyboard);
        }
    }
}