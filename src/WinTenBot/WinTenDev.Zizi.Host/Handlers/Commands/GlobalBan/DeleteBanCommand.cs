using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Telegram;
using WinTenDev.Zizi.Host.Tools;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Host.Handlers.Commands.GlobalBan
{
    public class DeleteBanCommand : CommandBase
    {
        private readonly GlobalBanService _globalBanService;
        private readonly TelegramService _telegramService;

        public DeleteBanCommand(GlobalBanService globalBanService, TelegramService telegramService)
        {
            _telegramService = telegramService;
            _globalBanService = globalBanService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = _telegramService.Message;
            var chatId = msg.Chat.Id;
            var fromId = msg.From.Id;
            var partedText = msg.Text.Split(" ");
            var param1 = partedText.ValueOfIndex(1); // User ID

            if (!_telegramService.IsSudoer())
            {
                Log.Warning("Not sudo can't execute this command..");
                return;
            }

            if (param1.IsNullOrEmpty())
            {
                await _telegramService.SendTextAsync("Spesifikasikan ID Pengguna yang mau di hapus dari Global Ban");
                return;
            }

            var repMsg = msg.ReplyToMessage;
            var userId = param1.ToInt();

            Log.Information("Execute Global DelBan");
            await _telegramService.SendTextAsync("Mempersiapkan..");

            var isBan = await _globalBanService.IsExist(userId);
            Log.Information("IsBan: {IsBan}", isBan);
            if (!isBan)
            {
                await _telegramService.EditAsync("Pengguna tidak di ban");
                return;
            }

            await _telegramService.EditAsync("Memperbarui informasi..");
            var save = await _globalBanService.DeleteBanAsync(userId);
            Log.Information("SaveBan: {Save}", save);

            await _telegramService.EditAsync("Memperbarui Cache..");
            await SyncUtil.SyncGBanToLocalAsync();

            await _telegramService.EditAsync("Pengguna berhasil di tambahkan");
        }
    }
}