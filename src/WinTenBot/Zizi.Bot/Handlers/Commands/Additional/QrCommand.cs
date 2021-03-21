using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Enums;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Additional
{
    public class QrCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public QrCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            Message repMsg = null;
            var data = msg.Text.GetTextWithoutCmd();

            if (msg.ReplyToMessage != null)
            {
                repMsg = msg.ReplyToMessage;
                data = repMsg.Text ?? repMsg.Caption;
            }

            if (data.IsNullOrEmpty())
            {
                var sendText = "<b>Generate QR from text or caption media</b>" +
                               "\n<b>Usage : </b><code>/qr</code> (In-Reply)" +
                               "\n                <code>/qr your text here</code> (In-Message)";
                await _telegramService.SendTextAsync(sendText);
                return;
            }

            InlineKeyboardMarkup keyboard = null;
            if (repMsg != null)
            {
                keyboard = new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl("Sumber", repMsg.GetMessageLink())
                );
            }

            var urlQr = data.GenerateUrlQrApi();
            await _telegramService.SendMediaAsync(urlQr.ToString(), MediaType.Photo, replyMarkup: keyboard);
        }
    }
}