using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Additional
{
    public class CovidCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly LmaoCovidService _lmaoCovidService;

        public CovidCommand(
            TelegramService telegramService,
            LmaoCovidService lmaoCovidService
            )
        {
            _telegramService = telegramService;
            _lmaoCovidService = lmaoCovidService;
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

                sendText = await _lmaoCovidService.GetCovidAll();
            }
            else
            {
                Log.Information("Getting Covid info by Region: {Part1}", part1);
                sendText = await _lmaoCovidService.GetCovidByCountry(part1);
            }

            await _telegramService.EditAsync(sendText);
        }
    }
}