using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.GlobalBan
{
    public class GlobalBanCommand : CommandBase
    {
        private GlobalBanService _globalBanService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;
            _globalBanService = new GlobalBanService(msg);

            var chatId = msg.Chat.Id;
            var fromId = msg.From.Id;
            var partedText = msg.Text.Split(" ");
            var param0 = partedText.ValueOfIndex(0) ?? "";
            var param1 = partedText.ValueOfIndex(1) ?? "";
            var userId = param1;
            var reason = msg.Text
                .Replace(param0, "", StringComparison.CurrentCulture)
                .Replace(param1, "", StringComparison.CurrentCulture).Trim();

            if (!fromId.IsSudoer())
            {
                await _telegramService.SendTextAsync("Anda haram melakukan ini")
                    .ConfigureAwait(false);
                return;
            }

            if (param1 == "sync")
            {
                await _telegramService.SendTextAsync("Memperbarui cache..")
                    .ConfigureAwait(false);
                await SyncUtil.SyncGBanToLocalAsync()
                    .ConfigureAwait(false);
                await _telegramService.EditAsync("Selesai memperbarui..")
                    .ConfigureAwait(false);
                return;
            }

            if (msg.ReplyToMessage == null)
            {
                if (param1.IsNullOrEmpty())
                {
                    await _telegramService.SendTextAsync("Balas seseorang yang mau di ban")
                        .ConfigureAwait(false);
                    return;
                }

                userId = param1;
                reason = msg.Text
                    .Replace(param0, "", StringComparison.CurrentCulture)
                    .Replace(param1, "", StringComparison.CurrentCulture).Trim();
            }
            else
            {
                var repMsg = msg.ReplyToMessage;
                userId = repMsg.From.Id.ToString();
                reason = msg.Text
                    .Replace(param0, "", StringComparison.CurrentCulture).Trim();
            }

            Log.Information("Execute Global Ban");
            await _telegramService.SendTextAsync("Mempersiapkan..")
                .ConfigureAwait(false);
            // await _telegramService.DeleteAsync(msg.MessageId)
            // .ConfigureAwait(false);

            var banData = new GlobalBanData()
            {
                UserId = userId.ToInt(),
                BannedBy = fromId,
                BannedFrom = chatId,
                ReasonBan = reason.IsNullOrEmpty() ? "no-reason" : reason
            };

            var isBan = await _globalBanService.IsExist(banData)
                .ConfigureAwait(false);

            if (isBan)
            {
                await _telegramService.EditAsync("Pengguna sudah di ban")
                    .ConfigureAwait(false);
                return;
            }

            await _telegramService.EditAsync("Menyimpan informasi..")
                .ConfigureAwait(false);
            var save = await _globalBanService.SaveBanAsync(banData)
                .ConfigureAwait(false);

            // var save = await _globalBanService.SaveBanAsync(data);
            Log.Information($"SaveBan: {save}");

            await _telegramService.EditAsync("Memperbarui cache.")
                .ConfigureAwait(false);
            await SyncUtil.SyncGBanToLocalAsync()
                .ConfigureAwait(false);


            await _telegramService.EditAsync("Pengguna berhasil di tambahkan.")
                .ConfigureAwait(false);

            // await _telegramService.DeleteAsync(delay: 3000)
            // .ConfigureAwait(false);
        }
    }
}