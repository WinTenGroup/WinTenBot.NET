using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Services;
using WinTenBot.Telegram;

namespace WinTenBot.Handlers.Commands.Metrics
{
    public class StorageCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            if (!_telegramService.IsSudoer())
            {
                return;
            }

            await _telegramService.SendTextAsync("<b>Storage Sense</b>")
                .ConfigureAwait(false);

            // var cachePath = BotSettings.PathCache.DirSize();
            // await _telegramService.AppendTextAsync($"<b>Cache Size: </b> {cachePath}")
            //     .ConfigureAwait(false);
        }
    }
}