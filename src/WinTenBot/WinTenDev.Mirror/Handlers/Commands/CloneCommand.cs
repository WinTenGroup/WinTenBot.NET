using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Core.Models.Settings;
using Zizi.Core.Services;
using Zizi.Core.Utils;
using Zizi.Core.Utils.GoogleCloud;

namespace WinTenDev.Mirror.Handlers.Commands
{
    public class CloneCommand : CommandBase
    {
        private TelegramService _telegramService;
        private readonly AppConfig _appConfig;
        private readonly DriveService _driveService;

        public CloneCommand(AppConfig appConfig, DriveService driveService)
        {
            _appConfig = appConfig;
            _driveService = driveService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context, _appConfig);
            var texts = _telegramService.MessageTextParts;
            var url = texts.ValueOfIndex(1);

            if (url == null)
            {
                await _telegramService.SendTextAsync("Masukkan URL yang akan di clone!");
                return;
            }

            await _telegramService.SendTextAsync("Cloning..");

            await _telegramService.CloneLink(_driveService, url);
        }
    }
}