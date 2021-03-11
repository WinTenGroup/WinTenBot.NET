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
        }

        public async Task<bool> ExecuteAsync()
        {
            Log.Information("Receiving Verify Callback");

            var callbackData = CallbackQuery.Data;
            var fromId = CallbackQuery.From.Id;
            Log.Information("CallbackData: {CallbackData} from {FromId}", callbackData, fromId);

            var partCallbackData = callbackData.Split(" ");
            var action = partCallbackData.ValueOfIndex(1);
            var target = partCallbackData.ValueOfIndex(2).ToInt();
            var isAdmin = await Telegram.IsAdminGroup(fromId);

            if (!isAdmin)
            {
                Log.Information("UserId: {FromId} is not Admin in this chat!", fromId);
                return false;
            }

            switch (action)
            {
                case "remove-warn":
                    Log.Information("Removing warn for {Target}", target);
                    await Telegram.RemoveWarnMemberStatAsync(target);
                    await Telegram.EditMessageCallback($"Peringatan untuk UserID: {target} sudah di hapus");
                    break;

                default:
                    Log.Information("Action {Action} is undefined", action);
                    break;
            }

            await Telegram.AnswerCallbackQueryAsync("Succed!");

            return true;
        }
    }
}