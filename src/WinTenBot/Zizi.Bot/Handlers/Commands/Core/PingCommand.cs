using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    internal class PingCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public PingCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var keyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("Ping", "PONG")
            );

            await _telegramService.AppendTextAsync("ℹ️ Pong!!");

            if (!_telegramService.IsPrivateChat || !_telegramService.IsFromSudoer())
            {
                Log.Warning("This not Sudo or not in private chat!");
                return;
            }

            await _telegramService.AppendTextAsync("🎛 <b>Engine info.</b>");

            var getWebHookInfo = await _telegramService.Client.GetWebhookInfoAsync(cancellationToken);
            if (getWebHookInfo.Url.IsNullOrEmpty())
            {
                // sendText += "\n\n<i>Bot run in Poll mode.</i>";
                await _telegramService.AppendTextAsync("\n<i>Bot run in Poll mode.</i>", keyboard);
            }
            else
            {
                var hookInfo = "\n<i>Bot run in Webhook mode.</i>" +
                               $"\nUrl WebHook: {getWebHookInfo.Url}" +
                               $"\nUrl Custom Cert: {getWebHookInfo.HasCustomCertificate}" +
                               $"\nAllowed Updates: {getWebHookInfo.AllowedUpdates}" +
                               $"\nPending Count: {getWebHookInfo.PendingUpdateCount}" +
                               $"\nMaxConnection: {getWebHookInfo.MaxConnections}" +
                               $"\nLast Error: {getWebHookInfo.LastErrorDate:yyyy-MM-dd hh:mm:ss zz}" +
                               $"\nError Message: {getWebHookInfo.LastErrorMessage}";

                await _telegramService.AppendTextAsync(hookInfo, keyboard);
            }
        }
    }
}