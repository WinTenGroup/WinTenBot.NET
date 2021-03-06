using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Commands.Additional
{
    public class NudityCommand : CommandBase
    {
        private TelegramService _telegramService;
        private DeepAiService _deepAiService;

        public NudityCommand(DeepAiService deepAiService)
        {
            _deepAiService = deepAiService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;
            var partsMsg = msg.Text.GetTextWithoutCmd().Split("|").ToArray();

            var repMsg = msg.ReplyToMessage;

            if (repMsg == null || repMsg.Type != MessageType.Photo)
            {
                await _telegramService.SendTextAsync("Silakan balas pesan Gambar yang mau dicek");
                Log.Debug("Processing {0} completed in {1}", nameof(NudityCommand), sw.Elapsed);

                sw.Stop();
                return;
            }

            await _telegramService.SendTextAsync("Sedang memeriksa Nudity..");

            var fileName = await _telegramService.DownloadFileAsync("nsfw-check");

            var result = await _deepAiService.NsfwDetectCoreAsync(fileName);
            var output = result.Output;

            var text = $"NSFW Score: {output.NsfwScore}" +
                       $"\n\nPowered by https://deepai.org";

            await _telegramService.EditAsync(text, disableWebPreview: true);
        }
    }
}