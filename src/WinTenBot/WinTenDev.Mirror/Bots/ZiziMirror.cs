using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace WinTenDev.Mirror.Bots
{
    public class ZiziMirror : BotBase
    {
        public ZiziMirror(IOptions<BotOptions<ZiziMirror>> options) : base(options.Value)
        {
        }

        public ZiziMirror(string username, ITelegramBotClient client) : base(username, client)
        {
        }

        public ZiziMirror(string username, string token) : base(username, token)
        {
        }

        public ZiziMirror(IBotOptions options) : base(options)
        {
        }
    }
}