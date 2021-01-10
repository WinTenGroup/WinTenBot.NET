using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Core.Models.Settings;
using Zizi.Core.Services;

namespace Zizi.Mirror.Handlers
{
    public class NewUpdateHandler : IUpdateHandler
    {
        private TelegramService _telegramService;
        private AppConfig _appConfig;

        public NewUpdateHandler(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context,_appConfig);

            Log.Debug("New Update..");

            await next(context, cancellationToken);
        }
    }
}