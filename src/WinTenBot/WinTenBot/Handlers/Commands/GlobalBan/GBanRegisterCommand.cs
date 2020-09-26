using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Services;
using WinTenBot.Telegram;

namespace WinTenBot.Handlers.Commands.GlobalBan
{
    public class GBanRegisterCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var message = _telegramService.Message;

            if (!await _telegramService.IsBeta().ConfigureAwait(false)) return;

            await _telegramService.SendTextAsync("Sedang memeriksa persyaratan")
                .ConfigureAwait(false);

            if (_telegramService.IsPrivateChat())
            {
                await _telegramService.EditAsync("Register Fed ES2 tidak dapat dilakukan di Private Chat.")
                    .ConfigureAwait(false);
                return;
            }

            if (!await _telegramService.IsAdminGroup().ConfigureAwait(false))
            {
                await _telegramService.EditAsync("Hanya admin yang dapat register ke Fed ES2.")
                    .ConfigureAwait(false);
                return;
            }

            var memberCount = await _telegramService.GetMemberCount().ConfigureAwait(false);
            if (memberCount < 197)
            {
                await _telegramService.EditAsync("Jumlah member di Grup ini kurang dari persyaratan minimum.")
                    .ConfigureAwait(false);
                return;
            }

            await _telegramService.RegisterGBanAdmin().ConfigureAwait(false);
        }
    }
}