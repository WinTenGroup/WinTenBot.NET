using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Callbacks
{
    public class ActionCallback
    {
        private TelegramService Telegram { get; set; }
        private CallbackQuery CallbackQuery { get; set; }

        public ActionCallback(TelegramService telegramService)
        {
            Telegram = telegramService;
            CallbackQuery = telegramService.Context.Update.CallbackQuery;
            Log.Information("Receiving Verify Callback");

            Parallel.Invoke(async () =>
                await ExecuteAsync().ConfigureAwait(false));
        }

        private async Task ExecuteAsync()
        {
            var callbackData = CallbackQuery.Data;
            var fromId = CallbackQuery.From.Id;
            Log.Information($"CallbackData: {callbackData} from {fromId}");

            var partCallbackData = callbackData.Split(" ");
            var action = partCallbackData.ValueOfIndex(1);
            var target = partCallbackData.ValueOfIndex(2).ToInt();
            var isAdmin = await Telegram.IsAdminGroup(fromId)
                .ConfigureAwait(false);

            if (!isAdmin)
            {
                Log.Information($"UserId: {fromId} is not Admin in this chat!");
                return;
            }

            switch (action)
            {
                case "remove-warn":
                    Log.Information($"Removing warn for {target}");
                    await Telegram.RemoveWarnMemberStatAsync(target)
                        .ConfigureAwait(false);
                    await Telegram.EditMessageCallback($"Peringatan untuk UserID: {target} sudah di hapus")
                        .ConfigureAwait(false);
                    break;

                default:
                    Log.Information($"Action {action} is undefined");
                    break;
            }

            await Telegram.AnswerCallbackQueryAsync("Succed!")
                .ConfigureAwait(false);
        }
    }
}