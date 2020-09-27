using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Rss
{
    public class ImportRssCommand : CommandBase
    {
        private RssService _rssService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            _rssService = new RssService(_telegramService.Message);
            var msg = _telegramService.Message;
            var msgId = msg.MessageId;
            var chatId = msg.Chat.Id;
            var msgText = msg.Text;
            var dateDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var isAdminOrPrivate = await _telegramService.IsAdminOrPrivateChat()
                .ConfigureAwait(false);
            if (!isAdminOrPrivate)
            {
                var send = "Maaf, hanya Admin yang dapat mengimport daftar RSS";
                await _telegramService.SendTextAsync(send)
                    .ConfigureAwait(false);
                return;
            }

            await _telegramService.AppendTextAsync("Sedang mempersiapkan")
                .ConfigureAwait(false);
            var filePath = $"{chatId}/rss-feed_{dateDate}_{msgId}.txt";
            filePath = await _telegramService.DownloadFileAsync(filePath)
                .ConfigureAwait(false);

            await _telegramService.AppendTextAsync("Sedang membuka berkas")
                .ConfigureAwait(false);
            var rssLists = await File.ReadAllLinesAsync(filePath, cancellationToken)
                .ConfigureAwait(false);
            foreach (var rssList in rssLists)
            {
                Log.Information($"Importing {rssList}");
                var data = new Dictionary<string, object>()
                {
                    {"url_feed", rssList},
                    {"chat_id", _telegramService.Message.Chat.Id},
                    {"from_id", _telegramService.Message.From.Id}
                };

                await _rssService.SaveRssSettingAsync(data)
                    .ConfigureAwait(false);
            }

            await _telegramService.AppendTextAsync($"Memeriksa RSS duplikat")
                .ConfigureAwait(false);
            await _rssService.DeleteDuplicateAsync()
                .ConfigureAwait(false);

            await _telegramService.AppendTextAsync($"{rssLists.Length} RSS berhasil di import")
                .ConfigureAwait(false);
        }
    }
}