using System.Threading;
using System.Threading.Tasks;
using SqlKata.Execution;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Telegram;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Words
{
    public class KataSyncCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly QueryFactory _queryFactory;
        private readonly WordFilterService _wordFilterService;

        public KataSyncCommand(
            QueryFactory queryFactory,
            TelegramService telegramService,
            WordFilterService wordFilterService
        )
        {
            _queryFactory = queryFactory;
            _telegramService = telegramService;
            _wordFilterService = wordFilterService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var isSudoer = _telegramService.IsSudoer();
            var isAdmin = await _telegramService.IsAdminGroup();

            if (!isSudoer)
            {
                return;
            }

            await _telegramService.DeleteAsync(_telegramService.Message.MessageId);

            await _telegramService.AppendTextAsync("Sedang mengsinkronkan Word Filter");
            await _wordFilterService.UpdateWordsCache();
// /            await _queryFactory.SyncWordToLocalAsync();


            await _telegramService.AppendTextAsync("Selesai mengsinkronkan.");

            await _telegramService.DeleteAsync(delay: 3000);
        }
    }
}