using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Scheduler;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Rss
{
    public class RssCtlCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = context.Update.Message;

            var isSudoer = _telegramService.IsSudoer();
            if (isSudoer)
            {
                var partedMsg = msg.Text.Split(" ");
                var param1 = partedMsg.ValueOfIndex(1);
                Log.Debug($"RssCtl Param1: {param1}");

                await _telegramService.AppendTextAsync("Access Granted")
                    .ConfigureAwait(false);
                switch (param1)
                {
                    case "start":
                        await _telegramService.AppendTextAsync("Starting RSS Service")
                            .ConfigureAwait(false);
                        RssScheduler.InitScheduler();
                        await _telegramService.AppendTextAsync("Start successfully.")
                            .ConfigureAwait(false);
                        break;

                    case "stop":
                        await _telegramService.AppendTextAsync("Stopping RSS Service")
                            .ConfigureAwait(false);
                            HangfireUtil.DeleteAllJobs();
                        await _telegramService.AppendTextAsync("Stop successfully.")
                            .ConfigureAwait(false);
                        break;
                }
            }
        }
    }
}