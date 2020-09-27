using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;

namespace Zizi.Bot.Bots
{
    public class ZiziBot : BotBase
    {
        public ZiziBot(IOptions<BotOptions<ZiziBot>> options) : base(options.Value)
        {
        }
    }
}
