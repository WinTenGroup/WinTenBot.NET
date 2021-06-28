using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Tools;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Commands.SpamLearning
{
    public class PredictCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public PredictCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var message = _telegramService.Message;

            if (message.ReplyToMessage != null)
            {
                var repMsg = message.ReplyToMessage;
                var repMsgText = repMsg.Text;

                await _telegramService.SendTextAsync("Sedang memprediksi pesan")
                    ;

                var isSpam = MachineLearning.PredictMessage(repMsgText);
                await _telegramService.EditAsync($"IsSpam: {isSpam}");

                return;
            }
            else
            {
                await _telegramService.SendTextAsync("Predicting message");
            }
        }
    }
}