using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Metrics
{
    public class StatsCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            if (!await _telegramService.IsBeta().ConfigureAwait(false)) return;

            var chatId = _telegramService.Message.Chat.Id;

            await _telegramService.GetStat().ConfigureAwait(false);
        }
    }
}