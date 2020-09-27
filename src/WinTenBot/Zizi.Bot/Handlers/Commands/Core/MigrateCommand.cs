using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class MigrateCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            await _telegramService.SendTextAsync("Migrate starting..")
                .ConfigureAwait(false);

            Thread.Sleep(3000);

            await _telegramService.EditAsync("Migrate finish..")
                .ConfigureAwait(false);
        }
    }
}