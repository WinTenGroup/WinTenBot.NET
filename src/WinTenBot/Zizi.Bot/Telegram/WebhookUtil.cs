using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class WebhookUtil
    {
        public static Task<WebhookInfo> GetWebhookInfo(this TelegramService telegramService)
        {
            return telegramService.Client.GetWebhookInfoAsync();
        }

        public static Task<WebhookInfo> GetWebhookInfo(this ITelegramBotClient client)
        {
            return client.GetWebhookInfoAsync();
        }
    }
}