using System.Threading;
using System.Threading.Tasks;
using SqlKata.Execution;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Words
{
    public class KataSyncCommand : CommandBase
    {
        private TelegramService _telegramService;
        private QueryFactory _queryFactory;

        public KataSyncCommand(QueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var isSudoer = _telegramService.IsSudoer();
            var isAdmin = await _telegramService.IsAdminGroup()
                .ConfigureAwait(false);

            if (!isSudoer)
            {
                return;
            }

            await _telegramService.DeleteAsync(_telegramService.Message.MessageId)
                .ConfigureAwait(false);

            await _telegramService.AppendTextAsync("Sedang mengsinkronkan Word Filter")
                .ConfigureAwait(false);
            await _queryFactory.SyncWordToLocalAsync().ConfigureAwait(false);

            await _telegramService.AppendTextAsync("Selesai mengsinkronkan.")
                .ConfigureAwait(false);

            await _telegramService.DeleteAsync(delay: 3000)
                .ConfigureAwait(false);
        }
    }
}