using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils.Telegram;
using WinTenDev.Zizi.Host.Telegram;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Group
{
    public class ReportCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public ReportCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            var sendText = "Balas pesan yg mau di report";

            if (msg.Chat.Type == ChatType.Private)
            {
                await _telegramService.SendTextAsync("Report hanya untuk grup saja");
                return;
            }

            if (msg.ReplyToMessage != null)
            {
                var repMsg = msg.ReplyToMessage;

                if (msg.From.Id != repMsg.From.Id)
                {
                    var mentionAdmins = await _telegramService.GetMentionAdminsStr();
                    var allListAdmin = await _telegramService.GetAllAdmins();
                    var allAdminId = allListAdmin.Select(a => a.User.Id);

                    var reporterNameLink = msg.GetFromNameLink();
                    var reportedNameLink = repMsg.GetFromNameLink();
                    var repMsgLink = repMsg.GetMessageLink();

                    sendText = $"Ada laporan nich." +
                               $"\n{reporterNameLink} melaporkan {reportedNameLink}" +
                               $"{mentionAdmins}";

                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Hapus", "PONG"),
                            InlineKeyboardButton.WithCallbackData("Tendang", "PONG"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Ban", "PONG"),
                            InlineKeyboardButton.WithUrl("Ke Pesan", repMsgLink),
                        }
                    });

                    await _telegramService.SendTextAsync(sendText)
                        ;
                    return;
                }

                sendText = "Melaporkan diri sendiri? 🤔";
            }

            await _telegramService.SendTextAsync(sendText);
        }
    }
}