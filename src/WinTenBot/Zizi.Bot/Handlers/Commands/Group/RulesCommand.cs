using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Common;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Commands.Group
{
    public class RulesCommand : CommandBase
    {
        private readonly SettingsService _settingsService;
        private readonly TelegramService _telegramService;

        public RulesCommand(SettingsService settingsService, TelegramService telegramService)
        {
            _settingsService = settingsService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);
            var msg = context.Update.Message;

            var chatId = _telegramService.ChatId;

            var sendText = "Under maintenance";
            if (msg.Chat.Type == ChatType.Private)
            {
                Log.Debug("Rules only for Group");
                return;
            }

            if (msg.From.Id.IsSudoer())
            {
                var settings = await _settingsService.GetSettingsByGroup(chatId);

                Log.Information("Settings: {0}", settings.ToJson(true));
                // var rules = settings.Rows[0]["rules_text"].ToString();
                var rules = settings.RulesText;
                Log.Debug("Rules: \n{0}", rules);
                sendText = rules;
            }

            await _telegramService.SendTextAsync(sendText);
        }
    }
}