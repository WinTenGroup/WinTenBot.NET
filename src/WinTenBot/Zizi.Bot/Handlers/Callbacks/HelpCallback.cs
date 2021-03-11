using System.Threading.Tasks;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Callbacks
{
    public class HelpCallback
    {
        private string CallBackData { get; set; }
        private readonly TelegramService _telegramService;

        public HelpCallback(TelegramService telegramService)
        {
            _telegramService = telegramService;
            CallBackData = telegramService.CallbackQuery.Data;
        }

        public async Task<bool> ExecuteAsync()
        {
            var partsCallback = CallBackData.SplitText(" ");
            var sendText = await partsCallback[1].LoadInBotDocs()
                .ConfigureAwait(false);
            Log.Information("Docs: {SendText}", sendText);
            var subPartsCallback = partsCallback[1].SplitText("/");

            Log.Information("SubParts: {V}", subPartsCallback.ToJson());
            var jsonButton = partsCallback[1];

            if (subPartsCallback.Count > 1)
            {
                jsonButton = subPartsCallback[0];

                switch (subPartsCallback[1])
                {
                    case "info":
                        jsonButton = subPartsCallback[1];
                        break;
                }
            }

            var keyboard = await $"Storage/Buttons/{jsonButton}.json".JsonToButton()
                .ConfigureAwait(false);
            
            await _telegramService.EditMessageCallback(sendText, keyboard);

            return true;
        }
    }
}