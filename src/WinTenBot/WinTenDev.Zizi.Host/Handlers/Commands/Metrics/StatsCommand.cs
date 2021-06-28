using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Telegram;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Metrics
{
    public class StatsCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public StatsCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            if (!await _telegramService.IsBeta()) return;

            var chatId = _telegramService.Message.Chat.Id;

            await _telegramService.GetStat();
        }
    }
}