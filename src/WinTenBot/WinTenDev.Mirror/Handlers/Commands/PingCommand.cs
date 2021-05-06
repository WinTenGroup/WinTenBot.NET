using System;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Core.Models.Settings;
using Zizi.Core.Services;

namespace WinTenDev.Mirror.Handlers.Commands
{
    public class PingCommand: CommandBase
    {
        private TelegramService _telegramService;
        private AppConfig _appConfig;
        private IEasyCachingProvider _easyCachingProvider;

        public PingCommand(AppConfig appConfig, IEasyCachingProvider easyCachingProvider)
        {
            _appConfig = appConfig;
            _easyCachingProvider = easyCachingProvider;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context, _appConfig);
            var message = _telegramService.Message;

            await _easyCachingProvider.SetAsync($"message{message.MessageId}", message, TimeSpan.FromMinutes(10));

            await _telegramService.SendTextAsync("Pong!");
        }
    }
}
