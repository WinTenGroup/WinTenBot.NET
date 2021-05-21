using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Telegram;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Group
{
    public class AfkCommand : CommandBase
    {
        private readonly AfkService _afkService;
        private readonly TelegramService _telegramService;

        public AfkCommand(TelegramService telegramService, AfkService afkService)
        {
            _afkService = afkService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            var fromId = _telegramService.FromId;
            var chatId = _telegramService.ChatId;

            var data = new Dictionary<string, object>()
            {
                {"user_id", fromId},
                {"chat_id", chatId},
                {"is_afk", 1},
                {"afk_start", DateTime.Now},
                {"afk_end", DateTime.Now}
            };

            var sendText = $"{msg.GetFromNameLink()} Sedang afk.";

            if (msg.Text.GetTextWithoutCmd().IsNotNullOrEmpty())
            {
                var afkReason = msg.Text.GetTextWithoutCmd();
                data.Add("afk_reason", afkReason);

                sendText += $"\n<i>{afkReason}</i>";
            }

            await _telegramService.SendTextAsync(sendText);
            await _afkService.SaveAsync(data);
            await _afkService.UpdateAfkByIdCacheAsync(fromId);
            // await _afkService.UpdateCacheAsync();
        }
    }
}