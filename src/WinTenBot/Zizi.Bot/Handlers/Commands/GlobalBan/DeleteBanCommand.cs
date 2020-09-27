using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.GlobalBan
{
    public class DeleteBanCommand : CommandBase
    {
        private GlobalBanService _globalBanService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;
            var chatId = msg.Chat.Id;
            var fromId = msg.From.Id;
            var partedText = msg.Text.Split(" ");
            var param1 = partedText.ValueOfIndex(1); // User ID

            _globalBanService = new GlobalBanService(msg);

            if (!fromId.IsSudoer())
            {
                await _telegramService.SendTextAsync("Anda haram melakukan ini")
                    .ConfigureAwait(false);
                return;
            }

            var repMsg = msg.ReplyToMessage;
            var userId = param1.ToInt();

            Log.Information("Execute Global DelBan");
            await _telegramService.SendTextAsync("Mempersiapkan..")
                .ConfigureAwait(false);
            // await _telegramService.DeleteAsync(msg.MessageId);

            var isBan = await _globalBanService.IsExist(userId)
                .ConfigureAwait(false);
            Log.Information($"IsBan: {isBan}");
            if (!isBan)
            {
                await _telegramService.EditAsync("Pengguna tidak di ban")
                    .ConfigureAwait(false);
                return;
            }

            await _telegramService.EditAsync("Memperbarui informasi..")
                .ConfigureAwait(false);
            var save = await _globalBanService.DeleteBanAsync(userId)
                .ConfigureAwait(false);
            Log.Information($"SaveBan: {save}");

            await _telegramService.EditAsync("Memperbarui Cache..")
                .ConfigureAwait(false);
            await Sync.SyncGBanToLocalAsync()
                .ConfigureAwait(false);

            await _telegramService.EditAsync("Misi berhasil.")
                .ConfigureAwait(false);

            // await _telegramService.DeleteAsync(delay: 3000)
            // .ConfigureAwait(false);
        }
    }
}