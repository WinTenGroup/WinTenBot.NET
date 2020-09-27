using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class OutCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = context.Update.Message;
            var partsMsg = msg.Text.GetTextWithoutCmd().Split("|").ToArray();

            var isSudoer = _telegramService.IsSudoer();
            if (isSudoer)
            {
                var sendText = "Maaf, saya harus keluar";

                if (partsMsg[1] != null)
                {
                    sendText += $"\n{partsMsg[1]}";
                }

                var chatId = partsMsg[0].ToInt64();
                Log.Information($"Target out: {chatId}");
                await _telegramService.SendTextAsync(sendText, customChatId: chatId)
                    .ConfigureAwait(false);
                await _telegramService.LeaveChat(chatId)
                    .ConfigureAwait(false);
            }
            else
            {
                await _telegramService.SendTextAsync("Kamu tidak punya hak akses.")
                    .ConfigureAwait(false);
            }
        }
    }
}