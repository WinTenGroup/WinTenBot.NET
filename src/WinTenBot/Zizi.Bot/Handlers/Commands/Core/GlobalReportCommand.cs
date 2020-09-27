using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class GlobalReportCommand:CommandBase
    {
        private TelegramService _telegramService;
        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;

            if (msg.ReplyToMessage != null)
            {
                var chatTitle = msg.Chat.Title;
                var chatId = msg.Chat.Id;
                var from = msg.From;
                var reason = msg.Text.GetTextWithoutCmd();

                var repMsg = msg.ReplyToMessage;
                var repFrom = repMsg.From;
                
                var msgBuild = new StringBuilder();

                msgBuild.AppendLine("‼️ <b>Global Report</b>");
                msgBuild.AppendLine($"<b>ChatTitle:</b> {chatTitle}");
                msgBuild.AppendLine($"<b>ChatID:</b> {chatId}");
                msgBuild.AppendLine($"<b>Reporter:</b> {from}");
                msgBuild.AppendLine($"<b>Reported:</b> {repFrom}");
                msgBuild.AppendLine($"<b>Reason:</b> {reason}");
                msgBuild.AppendLine($"\nTerima kasih sudah melaporkan!");

                var mentionAdmin = await _telegramService.GetMentionAdminsStr()
                    .ConfigureAwait(false);

                var isAdmin = await _telegramService.IsAdminGroup()
                    .ConfigureAwait(false);
                if (!isAdmin) msgBuild.AppendLine(mentionAdmin);
                
                var sendText = msgBuild.ToString().Trim();
                await _telegramService.ForwardMessageAsync(repMsg.MessageId)
                    .ConfigureAwait(false);
                await _telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
            }
            else
            {
                var sendText = "ℹ️ <b>Balas</b> pesan yang mau di laporkan" +
                               "\n\n<b>Catatan:</b> GReport (Global Report) akan melaporkan pengguna ke Tim @WinTenDev " +
                               "dan memanggil admin di Grup ini.";
                await _telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
            }
        }
    }
}