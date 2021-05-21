using System;
using System.Threading;
using System.Threading.Tasks;
using EasyCaching.Core;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Mirror.Host.Handlers.Commands
{
    public class PingCommand : CommandBase
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
            await _telegramService.AddUpdateContext(context);
            var message = _telegramService.Message;

            await _easyCachingProvider.SetAsync($"message{message.MessageId}", message, TimeSpan.FromMinutes(10));

            await _telegramService.SendTextAsync("Pong!");
        }
    }
}