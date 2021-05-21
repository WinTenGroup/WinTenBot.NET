using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;

namespace WinTenDev.Zizi.Models.Bots
{
    public class ZiziBot : BotBase
    {
        public ZiziBot(IOptions<BotOptions<ZiziBot>> options) : base(options.Value)
        {
        }
    }
}
