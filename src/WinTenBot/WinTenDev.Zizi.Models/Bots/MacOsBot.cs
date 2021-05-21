using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;

namespace WinTenDev.Zizi.Models.Bots
{
    public class MacOsBot : BotBase
    {
        public MacOsBot(IOptions<BotOptions<MacOsBot>> options) : base(options.Value)
        {
        }
    }
}