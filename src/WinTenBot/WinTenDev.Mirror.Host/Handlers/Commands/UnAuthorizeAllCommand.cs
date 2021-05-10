using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using LiteDB.Async;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Mirror.Host.Services;
using Zizi.Core.Models.Settings;
using Zizi.Core.Services;

namespace WinTenDev.Mirror.Host.Handlers.Commands
{
    public class UnAuthorizeAllCommand : CommandBase
    {
        private TelegramService _telegramService;
        private readonly AppConfig _appConfig;
        private LiteDatabaseAsync _liteDb;
        private IEasyCachingProvider _easyCachingProvider;
        private readonly AuthService _authService;

        public UnAuthorizeAllCommand(AppConfig appConfig, LiteDatabaseAsync liteDb, IEasyCachingProvider easyCachingProvider, AuthService authService)
        {
            _appConfig = appConfig;
            _easyCachingProvider = easyCachingProvider;
            _authService = authService;
            _liteDb = liteDb;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context, _appConfig);

            var fromId = _telegramService.FromId;
            var chatId = _telegramService.ChatId;

            if (!_telegramService.IsFromSudo)
            {
                Log.Information("User ID: {0} isn't sudo!", fromId);

                await _telegramService.SendTextAsync("You can't change authorization for this chat!");
                return;
            }

            await _telegramService.SendTextAsync("UnAuthorizing all chat..");

            await _authService.UnAuth();

            await _telegramService.EditAsync("All Chat has been UnAuthorized!");
        }
    }
}