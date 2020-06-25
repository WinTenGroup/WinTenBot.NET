using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Common;
using WinTenBot.IO;
using WinTenBot.Services;
using WinTenBot.Tools;
using WinTenBot.Tools.GoogleCloud;

namespace WinTenBot.Handlers.Commands.Additional
{
    public class OcrCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var msg = _telegramService.Message;
            var chatId = msg.Chat.Id;

            if (msg.ReplyToMessage != null)
            {
                var repMsg = msg.ReplyToMessage;
                var msgId = repMsg.MessageId;
                
                if (repMsg.Photo != null)
                {
                    await _telegramService.SendTextAsync("Sedang memproses gambar")
                        .ConfigureAwait(false);
                    
                    var fileName = $"{chatId}/ocr-{msgId}.jpg";
                    Log.Information("Preparing photo");
                    var savedFile = await _telegramService.DownloadFileAsync(fileName)
                        .ConfigureAwait(false);

                    // var ocr = TesseractProvider.ScanImage(savedFile);
                    var ocr = GoogleVision.ScanText(savedFile);
                    // var ocr = await TesseractProvider.OcrSpace(savedFile)
                        // .ConfigureAwait(false);

                    // var txt = @$"<b>Scan Result</b>\n{ocr}";

                    if (ocr.IsNullOrEmpty())
                    {
                        ocr = "Tidak terdeteksi adanya teks di gambar tersebut";
                    }
                    
                    await _telegramService.EditAsync(ocr)
                        .ConfigureAwait(false);
                    
                    savedFile.GetDirectory().RemoveFiles("ocr");

                    return;
                }
            }

            await _telegramService.SendTextAsync("Silakan reply salah satu gambar")
                .ConfigureAwait(false);
        }
    }
}