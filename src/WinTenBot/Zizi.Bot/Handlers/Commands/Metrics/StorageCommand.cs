using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Commands.Metrics
{
    public class StorageCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public StorageCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            if (!_telegramService.IsSudoer())
            {
                return;
            }

            await _telegramService.SendTextAsync("<b>Storage Sense</b>");

            // var cachePath = BotSettings.PathCache.DirSize();
            // await _telegramService.AppendTextAsync($"<b>Cache Size: </b> {cachePath}");
        }
    }
}