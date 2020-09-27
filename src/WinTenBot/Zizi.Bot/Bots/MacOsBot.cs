using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;

namespace Zizi.Bot.Bots
{
    public class MacOsBot : BotBase
    {
        public MacOsBot(IOptions<BotOptions<MacOsBot>> options) : base(options.Value)
        {
        }
    }
}