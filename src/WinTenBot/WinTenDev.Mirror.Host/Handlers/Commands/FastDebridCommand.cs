using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Mirror.Host.Handlers.Commands
{
    public class FastDebridCommand : CommandBase
    {
        private TelegramService _telegramService;
        private AppConfig _appConfig;

        public FastDebridCommand(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var url = _telegramService.MessageTextParts.ValueOfIndex(1);

            if (url.IsNullOrEmpty())
            {
                await _telegramService.SendTextAsync("Sertakan Url yg akan di konversi");
                return;
            }

            var debrid = await FastDebridUtil.Convert2(url);
            Log.Debug("Debrid: {0}", debrid);

            await _telegramService.SendTextAsync($"Sedang mengkonversi: {debrid}");
        }
    }
}