using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Services;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Commands.Welcome
{
    public class SetWelcomeCommand : CommandBase
    {
        private SettingsService _settingsService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = context.Update.Message;
            _settingsService = new SettingsService(msg);

            if (msg.Chat.Type == ChatType.Private)
            {
                await _telegramService.SendTextAsync("Welcome hanya untuk grup saja")
                    .ConfigureAwait(false);
                return;
            }

            if (!await _telegramService.IsAdminGroup()
                .ConfigureAwait(false))
            {
                return;
            }

            await _telegramService.SendTextAsync("/setwelcome sudah di pisah menjadi /welmsg, /welbtn dan /weldoc")
                .ConfigureAwait(false);
        }
    }
}