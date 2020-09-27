using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Model;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers
{
    public class MediaReceivedHandler : IUpdateHandler
    {
        private MediaFilterService _mediaFilterService;
        private TelegramService _telegramService;

        public MediaReceivedHandler()
        {
            _mediaFilterService = new MediaFilterService();
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = context.Update.Message;

            Log.Information("Media received... ");
            var fileId = msg.GetFileId();
            var reducedFileId = msg.GetReducedFileId();

            // var isBan = await _mediaFilterService.IsExistInCache("file_id", msg.GetReducedFileId());
            var isBan = await _mediaFilterService.IsExist("file_id", fileId)
                .ConfigureAwait(false);
            if (isBan)
            {
                await _telegramService.DeleteAsync(msg.MessageId)
                    .ConfigureAwait(false);
            }

            Log.Information($"Media isBan: {isBan}");

            if (BotSettings.HostingEnvironment.IsProduction())
                await _mediaFilterService.UpdateCacheAsync()
                .ConfigureAwait(false);
            else
                Log.Information($"Update cache skipped because local Env");

            Log.Information("Media Filter complete.");

            await next(context, cancellationToken).ConfigureAwait(false);
        }
    }
}