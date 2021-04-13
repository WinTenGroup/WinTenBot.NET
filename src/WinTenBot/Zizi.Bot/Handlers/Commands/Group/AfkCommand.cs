using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;

namespace Zizi.Bot.Handlers.Commands.Group
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

            var data = new Dictionary<string, object>()
            {
                {"user_id", msg.From.Id},
                {"chat_id", msg.Chat.Id},
                {"is_afk", 1}
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
            await _afkService.UpdateCacheAsync();
        }
    }
}