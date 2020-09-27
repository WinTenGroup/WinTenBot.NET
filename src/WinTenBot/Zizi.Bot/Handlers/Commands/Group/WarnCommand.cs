using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Group
{
    public class WarnCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;

            if (!await _telegramService.IsAdminOrPrivateChat()
                .ConfigureAwait(false))
                return;

            if (msg.ReplyToMessage != null)
            {
                await _telegramService.WarnMemberAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                await _telegramService.SendTextAsync("Balas pengguna yang akan di Warn", replyToMsgId: msg.MessageId)
                    .ConfigureAwait(false);
            }
        }
    }
}