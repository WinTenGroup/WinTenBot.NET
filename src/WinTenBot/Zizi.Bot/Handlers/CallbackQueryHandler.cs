using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Handlers.Callbacks;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers
{
    public class CallbackQueryHandler : IUpdateHandler
    {
        private TelegramService _telegramService;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var callbackQuery = context.Update.CallbackQuery;
            _telegramService.CallBackMessageId = callbackQuery.Message.MessageId;

            Log.Debug("CallbackQuery: {0}", callbackQuery.ToJson(true));
            // Log.Information($"CallBackData: {cq.Data}");

            var partsCallback = callbackQuery.Data.SplitText(" ");
            Log.Debug("Callbacks: {0}", partsCallback.ToJson(true));

            switch (partsCallback[0]) // Level 0
            {
                case "pong":
                case "PONG":
                    var pingResult = new PingCallback(_telegramService);
                    Log.Information("PingResult: {0}", pingResult.ToJson(true));
                    break;

                case "action":
                    var callbackResult = new ActionCallback(_telegramService);
                    Log.Information($"ActionResult: {callbackResult.ToJson(true)}");
                    break;

                case "help":
                    var helpCallback = new HelpCallback(_telegramService);
                    Log.Information($"HelpResult: {helpCallback.ToJson(true)}");
                    break;

                case "setting":
                    var settingsCallback = new SettingsCallback(_telegramService);
                    Log.Information($"SettingsResult: {settingsCallback.ToJson(true)}");
                    break;

                case "verify":
                    var verifyCallback = new VerifyCallback(_telegramService);
                    Log.Information($"VerifyResult: {verifyCallback.ToJson(true)}");
                    break;
            }

            // await context.Bot.Client.AnswerCallbackQueryAsync(cq.Id, "PONG", true);

            await next(context, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}