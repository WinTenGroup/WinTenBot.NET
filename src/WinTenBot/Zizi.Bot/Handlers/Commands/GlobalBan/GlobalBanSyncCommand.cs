using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.GlobalBan
{
    public class GlobalBanSyncCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly GlobalBanService _globalBanService;

        public GlobalBanSyncCommand(
            GlobalBanService globalBanService,
            TelegramService telegramService
        )
        {
            _globalBanService = globalBanService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            if (!_telegramService.IsSudoer()) return;

            await _telegramService.SendTextAsync("Sedang sinkronisasi..");

            await _globalBanService.UpdateGBanCache();
            var rowCount = await SyncUtil.SyncGBanToLocalAsync();

            await _telegramService.EditAsync($"Sinkronisasi sebanyak {rowCount} selesai");
        }
    }
}