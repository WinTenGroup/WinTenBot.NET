using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Tools;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Host.Handlers.Commands.GlobalBan
{
    public class GlobalBanCommand : CommandBase
    {
        private readonly GlobalBanService _globalBanService;
        private readonly TelegramService _telegramService;

        public GlobalBanCommand(
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

            int userId;
            string reason;

            var msg = _telegramService.Message;


            var chatId = msg.Chat.Id;
            var fromId = msg.From.Id;
            var partedText = msg.Text.Split(" ");
            var param0 = partedText.ValueOfIndex(0) ?? "";
            var param1 = partedText.ValueOfIndex(1) ?? "";

            if (!_telegramService.IsFromSudo)
            {
                await _telegramService.SendTextAsync("Harap melakukan Registrasi sebelum GBan");
                return;
            }

            if (param1 == "sync")
            {
                await _telegramService.SendTextAsync("Memperbarui cache..");
                await _globalBanService.UpdateGBanCache();

                await _telegramService.EditAsync("Selesai memperbarui..");

                return;
            }

            if (msg.ReplyToMessage == null)
            {
                if (param1.IsNullOrEmpty())
                {
                    await _telegramService.SendTextAsync("Balas seseorang yang mau di ban");

                    return;
                }

                userId = param1.ToInt();
                reason = msg.Text;
                if (reason.IsNotNullOrEmpty())
                    reason = reason
                        .Replace(param0, "", StringComparison.CurrentCulture)
                        .Replace(param1, "", StringComparison.CurrentCulture)
                        .Trim();
            }
            else
            {
                var repMsg = msg.ReplyToMessage;
                userId = repMsg.From.Id;
                reason = msg.Text;

                if (reason.IsNotNullOrEmpty())
                    reason = reason
                        .Replace(param0, "", StringComparison.CurrentCulture)
                        .Trim();
            }

            Log.Information("Execute Global Ban");
            await _telegramService.SendTextAsync("Mempersiapkan..");

            var banData = new GlobalBanData()
            {
                UserId = userId.ToInt(),
                BannedBy = fromId,
                BannedFrom = chatId,
                ReasonBan = reason.IsNullOrEmpty() ? "no-reason" : reason
            };

            var isBan = await _globalBanService.IsExist(userId);

            if (isBan)
            {
                await _telegramService.EditAsync("Pengguna sudah di ban");

                return;
            }

            await _telegramService.EditAsync("Menyimpan informasi..");
            var save = await _globalBanService.SaveBanAsync(banData);

            Log.Information("SaveBan: {Save}", save);

            await _telegramService.EditAsync("Memperbarui cache.");
            await _globalBanService.UpdateGBanCache(userId);
            await SyncUtil.SyncGBanToLocalAsync();

            await _telegramService.EditAsync("Pengguna berhasil di tambahkan.");
        }
    }
}