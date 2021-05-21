using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Mirror.Host.Handlers
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
            await _telegramService.AddUpdateContext(context);

            Log.Debug("New Update..");

            await next(context, cancellationToken);
        }
    }
}