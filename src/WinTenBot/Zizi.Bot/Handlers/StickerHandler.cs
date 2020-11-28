using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers
{
    class StickerHandler : IUpdateHandler
    {
        private TelegramService _telegramService;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var msg = context.Update.Message;
            var incomingSticker = msg.Sticker;

            var chat = await _telegramService.GetChat().ConfigureAwait(false);
            var stickerSetName = chat.StickerSetName ?? "EvilMinds";
            var stickerSet = await _telegramService.GetStickerSet(stickerSetName).ConfigureAwait(false);

            var similarSticker = stickerSet.Stickers.FirstOrDefault(
                sticker => incomingSticker.Emoji.Contains(sticker.Emoji)
            );

            var replySticker = similarSticker ?? stickerSet.Stickers.First();

            var stickerFileId = replySticker.FileId;

            await _telegramService.SendSticker(stickerFileId).ConfigureAwait(false);
        }
    }
}