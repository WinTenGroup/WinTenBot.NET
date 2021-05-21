using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Callbacks
{
    public class PingCallback
    {
        private readonly TelegramService _telegramService;
        private readonly CallbackQuery _callbackQuery;

        public PingCallback(TelegramService telegramService)
        {
            _telegramService = telegramService;
            _callbackQuery = telegramService.CallbackQuery;

        }

        public async Task<bool> ExecuteAsync()
        {
            Log.Information("Receiving Ping callback");

            var callbackData = _callbackQuery.Data;
            Log.Debug("CallbackData: {0}", callbackData);

            var answerCallback = $"Callback: {callbackData}";

            await _telegramService.AnswerCallbackQueryAsync(answerCallback, showAlert: true);

            return true;
        }
    }
}