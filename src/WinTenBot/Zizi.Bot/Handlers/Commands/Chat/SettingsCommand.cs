using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Chat
{
    public class SettingsCommand : CommandBase
    {
        private TelegramService _telegramService;
        private SettingsService _settingsService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            _settingsService = new SettingsService(context.Update.Message);
            var message = _telegramService.Message;

            await _telegramService.DeleteAsync(message.MessageId)
                .ConfigureAwait(false);

            var adminOrPrivate = await _telegramService.IsAdminOrPrivateChat()
                .ConfigureAwait(false);
            if (adminOrPrivate)
            {
                await _telegramService.SendTextAsync("Sedang mengambil pengaturan..")
                    .ConfigureAwait(false);
                var settings = await _settingsService.GetSettingButtonByGroup()
                    .ConfigureAwait(false);

                var btnMarkup = await settings.ToJson().JsonToButton(chunk: 2)
                    .ConfigureAwait(false);
                Log.Debug($"Settings: {settings.Count}");

                await _telegramService.EditAsync("Settings Toggles", btnMarkup)
                    .ConfigureAwait(false);
            }
        }
    }
}