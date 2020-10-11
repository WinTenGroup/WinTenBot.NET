using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Additional
{
    public class WgetCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var message = _telegramService.Message;
            var messageText = _telegramService.Message.Text.GetTextWithoutCmd();
            var chatId = message.Chat.Id;
            var partsText = messageText.Split(" ");
            var param1 = partsText.ValueOfIndex(0);

            var isBeta = await _telegramService.IsBeta().ConfigureAwait(false);

            if (!_telegramService.IsSudoer())
            {
                if (isBeta)
                {
                    await _telegramService.SendTextAsync("Fitur Wget saat ini masih dibatasi.")
                        .ConfigureAwait(false);
                    return;
                }

                if (chatId != -1001272521285)
                {
                    await _telegramService.SendTextAsync("Fitur Wget dapat di gunakan di grup @WinTenMirror")
                        .ConfigureAwait(false);
                    return;
                }
            }

            if (param1.IsNullOrEmpty())
            {
                await _telegramService.SendTextAsync("Silakan sertakan tautan yang akan di download")
                    .ConfigureAwait(false);
                return;
            }

            await _telegramService.SendTextAsync($"Preparing download file " +
                                                 $"\nUrl: {param1}")
                .ConfigureAwait(false);

            if (param1.IsMegaUrl())
            {
                await MegaTransfer.DownloadFileAsync(_telegramService, param1)
                    .ConfigureAwait(false);
            }
            else if (param1.IsUptoboxUrl())
            {
                await _telegramService.DownloadUrlAsync().ConfigureAwait(false);
            }
            else
            {
                await _telegramService.DownloadFile(param1)
                    .ConfigureAwait(false);
            }
        }
    }
}