using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Zizi.Bot.Services.Features;

namespace Zizi.Bot.Telegram
{
    public static class StickerUtil
    {
        public static Task<StickerSet> GetStickerSet(this ITelegramBotClient client, string stickerSetName)
        {
            return client.GetStickerSetAsync(stickerSetName);
        }

        public static Task<StickerSet> GetStickerSet(this TelegramService telegramService, string stickerSetName)
        {
            var client = telegramService.Client;
            return client.GetStickerSetAsync(stickerSetName);
        }

        public static async Task SendSticker(this TelegramService telegramService, string stickerFileId)
        {
            var client = telegramService.Client;
            var chatId = telegramService.ChatId;

            await client.SendStickerAsync(chatId, stickerFileId);
        }
    }
}