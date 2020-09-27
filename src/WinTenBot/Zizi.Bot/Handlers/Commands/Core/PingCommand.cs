using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Core
{
    internal class PingCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var msg = _telegramService.MessageOrEdited;

            var keyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("Ping", "PONG")
            );

            await _telegramService.AppendTextAsync("ℹ️ Pong!!").ConfigureAwait(false);
            var isSudoer = msg.From.Id.IsSudoer();

            if (msg.Chat.Type == ChatType.Private && isSudoer)
            {
                // await "\n<b>Engine info.</b>".AppendTextAsync();
                await _telegramService.AppendTextAsync("🎛 <b>Engine info.</b>").ConfigureAwait(false);

                // var getWebHookInfo = await _chatProcessor.Client.GetWebhookInfoAsync(cancellationToken);
                var getWebHookInfo = await _telegramService.Client.GetWebhookInfoAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (getWebHookInfo.Url.IsNullOrEmpty())
                {
                    // sendText += "\n\n<i>Bot run in Poll mode.</i>";
                    await _telegramService.AppendTextAsync("\n<i>Bot run in Poll mode.</i>", keyboard).ConfigureAwait(false);
                }
                else
                {
                    var hookInfo = "\n<i>Bot run in Webhook mode.</i>" +
                                   $"\nUrl WebHook: {getWebHookInfo.Url}" +
                                   $"\nUrl Custom Cert: {getWebHookInfo.HasCustomCertificate}" +
                                   $"\nAllowed Updates: {getWebHookInfo.AllowedUpdates}" +
                                   $"\nPending Count: {getWebHookInfo.PendingUpdateCount}" +
                                   $"\nMaxConnection: {getWebHookInfo.MaxConnections}" +
                                   $"\nLast Error: {getWebHookInfo.LastErrorDate}" +
                                   $"\nError Message: {getWebHookInfo.LastErrorMessage}";

                    await _telegramService.AppendTextAsync(hookInfo, keyboard)
                        .ConfigureAwait(false);
                }
            }

            // await sendText.AppendTextAsync(keyboard);
        }
    }
}