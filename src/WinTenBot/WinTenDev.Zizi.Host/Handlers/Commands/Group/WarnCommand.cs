﻿using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Telegram;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Group
{
    public class WarnCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public WarnCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = _telegramService.Message;

            if (!_telegramService.IsAdminOrPrivateChat()) return;

            if (msg.ReplyToMessage != null)
            {
                await _telegramService.WarnMemberAsync();
            }
            else
            {
                await _telegramService.SendTextAsync("Balas pengguna yang akan di Warn", replyToMsgId: msg.MessageId);
            }
        }
    }
}