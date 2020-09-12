using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Services;
using WinTenBot.Telegram;

namespace WinTenBot.Handlers.Commands.Metrics
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