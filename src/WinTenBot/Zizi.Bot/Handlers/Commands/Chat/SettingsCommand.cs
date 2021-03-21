using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;

namespace Zizi.Bot.Handlers.Commands.Chat
{
    public class SettingsCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly SettingsService _settingsService;

        public SettingsCommand(
            SettingsService settingsService,
            TelegramService telegramService
        )
        {
            _telegramService = telegramService;
            _settingsService = settingsService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var message = _telegramService.Message;
            var chatId = _telegramService.ChatId;

            await _telegramService.DeleteAsync(message.MessageId);

            var adminOrPrivate = _telegramService.IsAdminOrPrivateChat();
            if (!adminOrPrivate)
            {
                Log.Warning("Settings command only for Admin group or Private chat");
                return;
            }

            await _telegramService.SendTextAsync("Sedang mengambil pengaturan..", replyToMsgId: 0);
            var settings = await _settingsService.GetSettingButtonByGroup(chatId);

            var btnMarkup = await settings.ToJson().JsonToButton(chunk: 2);
            Log.Debug("Settings: {Count}", settings.Count);

            await _telegramService.EditAsync("Settings Toggles", btnMarkup);
        }
    }
}