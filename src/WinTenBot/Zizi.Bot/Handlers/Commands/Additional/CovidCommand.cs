using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Additional
{
    public class CovidCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public CovidCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var txt = _telegramService.Message.Text;
            var partTxt = txt.SplitText(" ").ToArray();
            var part1 = partTxt.ValueOfIndex(1); // Country

            await _telegramService.SendTextAsync("🔍 Getting information..");

            string sendText;
            if (part1.IsNullOrEmpty())
            {
                Log.Information("Getting Covid info Global");

                sendText = await Covid.GetCovidAll();
            }
            else
            {
                Log.Information("Getting Covid info by Region: {Part1}", part1);
                sendText = await Covid.GetCovidByCountry(part1);
            }

            await _telegramService.EditAsync(sendText);
        }
    }
}