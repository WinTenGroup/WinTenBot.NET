using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Async;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Core.Models.Settings;
using Zizi.Core.Services;
using Zizi.Mirror.Utils;

namespace Zizi.Mirror.Handlers.Commands
{
    public class AuthorizeCommand : CommandBase
    {
        private TelegramService _telegramService;
        private AppConfig _appConfig;
        private LiteDatabaseAsync _liteDb;

        public AuthorizeCommand(AppConfig appConfig, LiteDatabaseAsync liteDb)
        {
            _appConfig = appConfig;
            _liteDb = liteDb;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context, _appConfig);

            var fromId = _telegramService.FromId;
            var chatId = _telegramService.ChatId;

            if (!_telegramService.IsFromSudo)
            {
                Log.Information("User ID: {0} isn't sudo!", fromId);

                await _telegramService.SendTextAsync("You can't authorize this chat!");
                return;
            }

            if (await _liteDb.IsAuth(chatId))
            {
                await _telegramService.SendTextAsync("Chat has been authorized!");
                return;
            }

            await _telegramService.SendTextAsync("Authorizing chat..");

            await _liteDb.SaveAuth(new AuthorizedChat()
            {
                ChatId = chatId,
                AuthorizedBy = fromId,
                CreatedAt = DateTime.Now
            });

            await _telegramService.EditAsync("Chat authorized!");
        }
    }
}