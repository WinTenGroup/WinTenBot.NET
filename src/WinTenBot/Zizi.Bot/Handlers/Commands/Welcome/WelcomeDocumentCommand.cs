using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Welcome
{
    public class WelcomeDocumentCommand : CommandBase
    {
        private TelegramService _telegramService;
        private SettingsService _settingsService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;
            _settingsService = new SettingsService(msg);

            if (msg.Chat.Type == ChatType.Private)
            {
                await _telegramService.SendTextAsync($"Welcome media hanya untuk grup saja")
                    .ConfigureAwait(false);
                return;
            }

            if (!await _telegramService.IsAdminGroup()
                .ConfigureAwait(false))
            {
                return;
            }

            if (msg.ReplyToMessage != null)
            {
                var repMsg = msg.ReplyToMessage;
                if (repMsg.GetFileId().IsNotNullOrEmpty())
                {
                    var mediaFileId = repMsg.GetFileId();
                    var mediaType = repMsg.Type;

                    await _telegramService.SendTextAsync("Sedang menyimpan Welcome Media..")
                        .ConfigureAwait(false);
                    Log.Information($"MediaId: {mediaFileId}");

                    await _settingsService.UpdateCell("welcome_media", mediaFileId).ConfigureAwait(false);
                    await _settingsService.UpdateCell("welcome_media_type", mediaType).ConfigureAwait(false);
                    Log.Information("Save media success..");

                    await _telegramService.EditAsync("Welcome Media berhasil di simpan." +
                                                     "\nKetik /welcome untuk melihat perubahan")
                        .ConfigureAwait(false);
                    return;
                }
                else
                {
                    await _telegramService.SendTextAsync("Media tidak terdeteksi di pesan yg di reply tersebut.")
                        .ConfigureAwait(false);
                    return;
                }
            }
            else
            {
                await _telegramService
                    .SendTextAsync("Balas sebuah gambar, video, atau dokumen yang akan di jadikan Welcome media")
                    .ConfigureAwait(false);
            }
        }
    }
}