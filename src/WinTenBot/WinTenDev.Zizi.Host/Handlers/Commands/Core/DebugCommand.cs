using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Core
{
    public class DebugCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public DebugCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            var json = msg.ToJson(true);

            Log.Information("Debug: {0}", json.Length.ToString());

            var sendText = $"Debug:\n {json}";
            await _telegramService.SendTextAsync(sendText);
        }
    }
}