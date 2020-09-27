using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Words
{
    public class KataSyncCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var isSudoer = _telegramService.IsSudoer();
            var isAdmin = await _telegramService.IsAdminGroup()
                .ConfigureAwait(false);

            if (isSudoer)
            {
                await _telegramService.DeleteAsync(_telegramService.Message.MessageId)
                    .ConfigureAwait(false);

                await _telegramService.AppendTextAsync("Sedang mengsinkronkan Word Filter")
                    .ConfigureAwait(false);
                await Sync.SyncWordToLocalAsync().ConfigureAwait(false);
                await _telegramService.AppendTextAsync("Selesai mengsinkronkan.")
                    .ConfigureAwait(false);

                await _telegramService.DeleteAsync(delay: 3000)
                    .ConfigureAwait(false);
            }
        }
    }
}