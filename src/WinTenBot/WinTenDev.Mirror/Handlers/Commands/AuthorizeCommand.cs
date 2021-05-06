using System;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using LiteDB.Async;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Mirror.Services;
using Zizi.Core.Models.Settings;
using Zizi.Core.Services;

namespace WinTenDev.Mirror.Handlers.Commands
{
    public class AuthorizeCommand : CommandBase
    {
        private TelegramService _telegramService;
        private readonly AppConfig _appConfig;
        private LiteDatabaseAsync _liteDb;
        private IEasyCachingProvider _easyCachingProvider;
        private readonly AuthService _authService;

        public AuthorizeCommand(AppConfig appConfig, LiteDatabaseAsync liteDb, IEasyCachingProvider easyCachingProvider, AuthService authService)
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

            if (await _authService.IsAuth(chatId))
            {
                await _telegramService.SendTextAsync("Chat has been authorized!");
                return;
            }

            if (!_telegramService.IsFromSudo)
            {
                Log.Information("User ID: {0} isn't sudo!", fromId);

                await _telegramService.SendTextAsync("You can't authorize this chat!");
                return;
            }

            await _telegramService.SendTextAsync("Authorizing chat..");

            await _authService.SaveAuth(new AuthorizedChat()
            {
                ChatId = chatId,
                AuthorizedBy = fromId,
                IsAuthorized = true,
                CreatedAt = DateTime.Now
            });

            await _telegramService.EditAsync("Chat has been authorized!");
        }
    }
}