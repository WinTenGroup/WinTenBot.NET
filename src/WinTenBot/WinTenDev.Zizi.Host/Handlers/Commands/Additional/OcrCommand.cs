using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Tools.GoogleCloud;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.IO;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Additional
{
    public class OcrCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public OcrCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = _telegramService.Message;
            var chatId = msg.Chat.Id;

            if (msg.ReplyToMessage != null)
            {
                var repMsg = msg.ReplyToMessage;
                var msgId = repMsg.MessageId;
                
                if (repMsg.Photo != null)
                {
                    await _telegramService.SendTextAsync("Sedang memproses gambar");
                    
                    var fileName = $"{chatId}/ocr-{msgId}.jpg";
                    Log.Information("Preparing photo");
                    var savedFile = await _telegramService.DownloadFileAsync(fileName);

                    var ocr = GoogleVision.ScanText(savedFile);


                    if (ocr.IsNullOrEmpty())
                    {
                        ocr = "Tidak terdeteksi adanya teks di gambar tersebut";
                    }
                    
                    await _telegramService.EditAsync(ocr);
                    
                    savedFile.GetDirectory().RemoveFiles("ocr");

                    return;
                }
            }

            await _telegramService.SendTextAsync("Silakan reply salah satu gambar");
        }
    }
}