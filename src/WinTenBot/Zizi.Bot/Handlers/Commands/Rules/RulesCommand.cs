using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Rules
{
    public class RulesCommand : CommandBase
    {
        private SettingsService _settingsService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            var msg = context.Update.Message;

            _telegramService = new TelegramService(context);
            _settingsService = new SettingsService(msg);


            var sendText = "Under maintenance";
            if (msg.Chat.Type != ChatType.Private)
            {
                if (msg.From.Id.IsSudoer())
                {
                    var settings = await _settingsService.GetSettingByGroup();
                    await _settingsService.UpdateCache();
                    Log.Information(settings.ToJson());
                    // var rules = settings.Rows[0]["rules_text"].ToString();
                    var rules = settings.RulesText;
                    Log.Information(rules);
                    sendText = rules;
                }
            }
            else
            {
                sendText = "Rules hanya untuk grup";
            }

            await _telegramService.SendTextAsync(sendText);
        }
    }
}