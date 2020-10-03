using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.SpamLearning
{
    public class LearnCommand : CommandBase
    {
        private TelegramService _telegramService;
        private LearningService _learningService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var message = _telegramService.Message;
            _learningService = new LearningService(message);

            if (!_telegramService.IsSudoer())
            {
                Log.Information("This user is not sudoer");
                return;
            }

            if (message.ReplyToMessage != null)
            {
                var repMessage = message.ReplyToMessage;
                var repText = repMessage.Text ?? repMessage.Caption;
                var param = message.Text.SplitText(" ").ToArray();
                var mark = param.ValueOfIndex(1);
                var opts = new List<string>()
                {
                    "spam", "ham"
                };

                if (!opts.Contains(mark))
                {
                    await _telegramService.SendTextAsync("Spesifikasikan spam atau ham (bukan spam)")
                        .ConfigureAwait(false);
                    return;
                }

                await _telegramService.SendTextAsync("Sedang memperlajari pesan")
                    .ConfigureAwait(false);
                var learnData = new LearnData()
                {
                    Message = repText.Replace("\n", " "),
                    Label = mark
                };

                if (LearningService.IsExist(learnData))
                {
                    Log.Information("This message has learned");
                    await _telegramService.EditAsync("Pesan ini mungkin sudah di tambahkan.")
                        .ConfigureAwait(false);
                    return;
                }

                await _learningService.Save(learnData).ConfigureAwait(false);

                await _telegramService.EditAsync("Memperbarui local dataset")
                    .ConfigureAwait(false);
                // MachineLearning.WriteToCsv();

                await _telegramService.EditAsync("Sedang mempelajari dataset")
                    .ConfigureAwait(false);
                await MachineLearning.SetupEngineAsync().ConfigureAwait(false);
                // BackgroundJob.Enqueue(() => LearningHelper.SetupEngine());
                
                await _telegramService.EditAsync("Pesan berhasil di tambahkan ke Dataset")
                    .ConfigureAwait(false);
                
                return;
            }
            else
            {
                await _telegramService.SendTextAsync("Sedang mempelajari dataset")
                    .ConfigureAwait(false);
                await MachineLearning.SetupEngineAsync()
                    .ConfigureAwait(false);
                
                await _telegramService.EditAsync("Training selesai")
                    .ConfigureAwait(false);
                
                return;
            }

            await _telegramService.SendTextAsync("Balas pesan yang ingin di pelajari")
                .ConfigureAwait(false);
        }
    }
}