using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using WinTenDev.Zizi.Host.Telegram;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Welcome
{
    public class SetWelcomeCommand : CommandBase
    {
        private readonly SettingsService _settingsService;
        private readonly TelegramService _telegramService;

        public SetWelcomeCommand(TelegramService telegramService, SettingsService settingsService)
        {
            _telegramService = telegramService;
            _settingsService = settingsService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;

            if (msg.Chat.Type == ChatType.Private)
            {
                await _telegramService.SendTextAsync("Welcome hanya untuk grup saja");
                return;
            }

            if (!await _telegramService.IsAdminGroup()
                )
            {
                return;
            }

            await _telegramService.SendTextAsync("/setwelcome sudah di pisah menjadi /welmsg, /welbtn dan /weldoc");
        }
    }
}