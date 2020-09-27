using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers
{
    class StickerHandler : IUpdateHandler
    {
        private TelegramService _telegramService;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            Message msg = context.Update.Message;
            Sticker incomingSticker = msg.Sticker;

            var chat = await _telegramService.GetChat();
            var stickerSetName = chat.StickerSetName ?? "EvilMinds";
            StickerSet evilMindsSet = await context.Bot.Client.GetStickerSetAsync(stickerSetName, cancellationToken);

            Sticker similarEvilMindSticker = evilMindsSet.Stickers.FirstOrDefault(
                sticker => incomingSticker.Emoji.Contains(sticker.Emoji)
            );

            Sticker replySticker = similarEvilMindSticker ?? evilMindsSet.Stickers.First();

            await context.Bot.Client.SendStickerAsync(
                msg.Chat,
                replySticker.FileId,
                replyToMessageId: msg.MessageId, cancellationToken: cancellationToken);
        }
    }
}