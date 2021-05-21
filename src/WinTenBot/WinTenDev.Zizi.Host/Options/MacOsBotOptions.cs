﻿using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace WinTenDev.Zizi.Host.Options
{
    public class MacOsBotOptions<TBot> : BotOptions<TBot>
        where TBot : IBot
    {
        public string WebhookDomain { get; set; }
    }
}