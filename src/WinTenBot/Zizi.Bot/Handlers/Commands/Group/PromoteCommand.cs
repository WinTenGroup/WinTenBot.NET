using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Commands.Group
{
    public class PromoteCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public PromoteCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            if (msg.ReplyToMessage != null)
            {
                msg = msg.ReplyToMessage;
            }

            var userId = msg.From.Id;
            var nameLink = msg.GetFromNameLink();

            var sendText = $"{nameLink} berhasil menjadi Admin";

            var promote = await _telegramService.PromoteChatMemberAsync(userId);
            if (!promote.IsSuccess)
            {
                var errorCode = promote.ErrorCode;
                var errorMessage = promote.ErrorMessage;

                sendText = $"Promote {nameLink} gagal" +
                           $"\nPesan: {errorMessage}";
            }

            await _telegramService.SendTextAsync(sendText);
        }
    }
}