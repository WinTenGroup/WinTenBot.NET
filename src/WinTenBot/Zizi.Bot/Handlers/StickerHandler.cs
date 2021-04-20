using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers
{
    class StickerHandler : IUpdateHandler
    {
        private readonly TelegramService _telegramService;

        public StickerHandler(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            var incomingSticker = msg.Sticker;

            var chat = await _telegramService.GetChat();
            var stickerSetName = chat.StickerSetName ?? "EvilMinds";
            var stickerSet = await _telegramService.GetStickerSet(stickerSetName);

            var similarSticker = stickerSet.Stickers.FirstOrDefault(
                sticker => incomingSticker.Emoji.Contains(sticker.Emoji)
            );

            var replySticker = similarSticker ?? stickerSet.Stickers.First();

            var stickerFileId = replySticker.FileId;

            await _telegramService.SendSticker(stickerFileId);
        }
    }
}