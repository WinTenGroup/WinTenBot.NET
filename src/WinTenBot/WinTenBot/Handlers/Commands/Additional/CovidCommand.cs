using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Common;
using WinTenBot.Services;
using WinTenBot.Tools;

namespace WinTenBot.Handlers.Commands.Additional
{
    public class CovidCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var txt = _telegramService.Message.Text;
            var partTxt = txt.SplitText(" ").ToArray();
            var part1 = partTxt.ValueOfIndex(1); // Country

            await _telegramService.SendTextAsync("🔍 Getting information..")
                .ConfigureAwait(false);

            var sendText = "";
            if (part1.IsNullOrEmpty())
            {
                Log.Information("Getting Covid info Global");
                // var sendText = await CovidHelper.GetCovidUpdatesAsync();
                sendText = await Covid.GetCovidAll()
                    .ConfigureAwait(false);
            }
            else
            {
                Log.Information($"Getting Covid info by Region: {part1}");
                sendText = await Covid.GetCovidByCountry(part1)
                    .ConfigureAwait(false);
            }
            
            await _telegramService.EditAsync(sendText)
                .ConfigureAwait(false);
            
        }
    }
}