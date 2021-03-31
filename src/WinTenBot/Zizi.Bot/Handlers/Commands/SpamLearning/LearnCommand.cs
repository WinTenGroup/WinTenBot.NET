using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Telegram;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.SpamLearning
{
    public class LearnCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly LearningService _learningService;

        public LearnCommand(TelegramService telegramService, LearningService learningService)
        {
            _telegramService = telegramService;
            _learningService = learningService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var message = _telegramService.Message;

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
                var opts = new List<string>
                {
                    "spam", "ham"
                };

                if (!opts.Contains(mark))
                {
                    await _telegramService.SendTextAsync("Spesifikasikan spam atau ham (bukan spam)");
                    return;
                }

                await _telegramService.SendTextAsync("Sedang memperlajari pesan");
                var learnData = new LearnData
                {
                    Message = repText.Replace("\n", " "),
                    Label = mark,
                    ChatId = _telegramService.ChatId,
                    FromId = _telegramService.FromId
                };

                if (_learningService.IsExist(learnData))
                {
                    Log.Information("This message has learned");
                    await _telegramService.EditAsync("Pesan ini mungkin sudah di tambahkan.");
                    return;
                }

                await _learningService.Save(learnData);

                await _telegramService.EditAsync("Memperbarui local dataset");

                await _telegramService.EditAsync("Sedang mempelajari dataset");
                await MachineLearning.SetupEngineAsync();

                await _telegramService.EditAsync("Pesan berhasil di tambahkan ke Dataset");
            }
            else
            {
                await _telegramService.SendTextAsync("Sedang mempelajari dataset");
                await MachineLearning.SetupEngineAsync();

                await _telegramService.EditAsync("Training selesai");
            }
        }
    }
}