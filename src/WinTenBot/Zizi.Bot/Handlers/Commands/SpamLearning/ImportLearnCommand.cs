using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.SpamLearning
{
    public class ImportLearnCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var message = _telegramService.Message;
            var msgText = message.Text.Split(' ');
            var param1 = "\t";
            // var param1 = msgText.ValueOfIndex(1).Trim() ?? ",";

            if (!_telegramService.IsSudoer())
            {
                Log.Information("This user is not sudoer");
                return;
            }

            if (message.ReplyToMessage != null)
            {
                await _telegramService.SendTextAsync("Sedang mengimport dataset")
                    .ConfigureAwait(false);

                var repMessage = message.ReplyToMessage;
                if (repMessage.Document != null)
                {
                    var document = repMessage.Document;
                    var chatId = message.Chat.Id.ToString().TrimStart('-');
                    var msgId = repMessage.MessageId;
                    var fileName = document.FileName;
                    var filePath = $"learn-dataset-{chatId}-{msgId}-{fileName}";
                    var savedFile = await _telegramService.DownloadFileAsync(filePath)
                        .ConfigureAwait(false);

                    await _telegramService.ImportCsv(savedFile, param1).ConfigureAwait(false);

                    await _telegramService.EditAsync("Sedang mempelajari dataset")
                        .ConfigureAwait(false);
                    await MachineLearning.SetupEngineAsync()
                        .ConfigureAwait(false);

                    await _telegramService.EditAsync("Import selesai")
                        .ConfigureAwait(false);
                }
                else
                {
                    var typeHint = "File yang mau di import harus berupa dokumen bertipe csv, tsv atau sejenis";
                    await _telegramService.SendTextAsync(typeHint)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                await _telegramService.SendTextAsync("Balas file yang mau di import")
                    .ConfigureAwait(false);
            }
        }
    }
}