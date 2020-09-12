using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Services;
using WinTenBot.Telegram;
using WinTenBot.Tools;

namespace WinTenBot.Handlers.Commands.GlobalBan
{
    public class GlobalBanSyncCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            
            if(!_telegramService.IsSudoer()) return;

            await _telegramService.SendTextAsync("Sedang sinkronisasi..")
                .ConfigureAwait(false);

            var rowCount = await Sync.SyncGBanToLocalAsync()
                .ConfigureAwait(false);

            await _telegramService.EditAsync($"Sinkronisasi sebanyak {rowCount} selesai")
                .ConfigureAwait(false);
        }
    }
}