using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace Zizi.Core.Models
{
    public class CustomBotOptions<TBot> : BotOptions<TBot>
        where TBot : IBot
    {
        public string WebhookDomain { get; set; }
    }
}
