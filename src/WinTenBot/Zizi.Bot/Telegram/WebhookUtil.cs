using System.Threading.Tasks;
using Serilog;
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

        public static async Task NotifyPendingCount(this TelegramService telegramService, int pendingLimit = 100)
        {
            var webhookInfo = await telegramService.GetWebhookInfo();

            var pendingCount = webhookInfo.PendingUpdateCount;
            if (pendingCount != pendingLimit)
            {
                await telegramService.SendEventCoreAsync($"Pending Count larger than {pendingLimit}", repToMsgId: 0);
            }
            else
            {
                Log.Information("Pending count is under {0}", pendingLimit);
            }
        }
    }
}