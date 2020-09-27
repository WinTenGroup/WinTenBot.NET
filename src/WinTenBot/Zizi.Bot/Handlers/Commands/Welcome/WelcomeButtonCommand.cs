using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Welcome
{
    public class WelcomeButtonCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;
            var targetName = "button";

            if (msg.Chat.Type == ChatType.Private)
            {
                await _telegramService.SendTextAsync($"Welcome {targetName} hanya untuk grup saja")
                    .ConfigureAwait(false);
                return;
            }

            if (!await _telegramService.IsAdminGroup()
                .ConfigureAwait(false))
            {
                return;
            }

            await _telegramService.SaveWelcome(targetName)
                .ConfigureAwait(false);
        }
    }
}