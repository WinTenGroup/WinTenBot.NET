﻿using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Group
{
    public class PinCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public PinCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = _telegramService.MessageOrEdited;
            var client = context.Bot.Client;

            var sendText = "Balas pesan yang akan di pin";

            var isAdmin = _telegramService.IsFromAdmin;
            if (!isAdmin)
            {
                Log.Warning("Pin message only for Admin on Current Chat");
                await _telegramService.DeleteAsync(msg.MessageId);
                return;
            }

            if (msg.ReplyToMessage != null)
            {
                await client.PinChatMessageAsync(
                    msg.Chat.Id,
                    msg.ReplyToMessage.MessageId,
                    cancellationToken: cancellationToken);
                return;
            }

            await _telegramService.SendTextAsync(sendText, replyToMsgId: msg.MessageId);
        }
    }
}