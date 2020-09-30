using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class HelpCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var sendText = "Untuk mendapatkan bantuan klik tombol dibawah ini";
            var urlStart = await _telegramService.GetUrlStart("start=help")
                .ConfigureAwait(false);
            var ziziDocs = "https://docs.zizibot.azhe.space";

            var keyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl("Dapatkan bantuan", ziziDocs)
            );

            // if (_telegramService.IsPrivateChat())
            // {
            //     sendText = await "home".LoadInBotDocs()
            //         .ConfigureAwait(false);
            //     keyboard = await "Storage/Buttons/home.json".JsonToButton()
            //         .ConfigureAwait(false);
            // }

            await _telegramService.SendTextAsync(sendText, keyboard)
                .ConfigureAwait(false);
        }
    }
}