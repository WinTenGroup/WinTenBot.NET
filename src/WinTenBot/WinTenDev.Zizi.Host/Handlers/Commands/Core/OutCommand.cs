using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Telegram;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Core
{
    public class OutCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public OutCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            var partsMsg = _telegramService.MessageTextParts;

            var isSudoer = _telegramService.IsSudoer();
            if (!isSudoer)
            {
                return;
            }

            var sendText = "Maaf, saya harus keluar";

            if (partsMsg.ValueOfIndex(2) != null)
            {
                sendText += $"\n{partsMsg.ValueOfIndex(1)}";
            }

            var chatId = partsMsg.ValueOfIndex(1).ToInt64();
            Log.Information("Target out: {ChatId}", chatId);

            await _telegramService.SendTextAsync(sendText, customChatId: chatId);
            await _telegramService.LeaveChat(chatId);
        }
    }
}