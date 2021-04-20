using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Callbacks
{
    public class ActionCallback
    {
        private readonly TelegramService _telegramService;
        private readonly CallbackQuery _callbackQuery;

        public ActionCallback(TelegramService telegramService)
        {
            _telegramService = telegramService;
            _callbackQuery = telegramService.Context.Update.CallbackQuery;
        }

        public async Task<bool> ExecuteAsync()
        {
            Log.Information("Receiving Verify Callback");

            var callbackData = _callbackQuery.Data;
            var fromId = _callbackQuery.From.Id;
            Log.Information("CallbackData: {CallbackData} from {FromId}", callbackData, fromId);

            var partCallbackData = callbackData.Split(" ");
            var action = partCallbackData.ValueOfIndex(1);
            var target = partCallbackData.ValueOfIndex(2).ToInt();
            var isAdmin = await _telegramService.IsAdminGroup(fromId);

            if (!isAdmin)
            {
                Log.Information("UserId: {FromId} is not Admin in this chat!", fromId);
                return false;
            }

            switch (action)
            {
                case "remove-warn":
                    Log.Information("Removing warn for {Target}", target);
                    await _telegramService.RemoveWarnMemberStatAsync(target);
                    await _telegramService.EditMessageCallback($"Peringatan untuk UserID: {target} sudah di hapus");
                    break;

                default:
                    Log.Information("Action {Action} is undefined", action);
                    break;
            }

            await _telegramService.AnswerCallbackQueryAsync("Succed!");

            return true;
        }
    }
}