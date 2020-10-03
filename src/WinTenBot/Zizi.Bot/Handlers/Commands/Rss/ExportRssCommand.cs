using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Telegram;
using Zizi.Bot.Enums;
using Zizi.Bot.Models;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Rss
{
    public class ExportRssCommand : CommandBase
    {
        private RssService _rssService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            _rssService = new RssService(_telegramService.Message);
            var msg = _telegramService.Message;
            var chatId = msg.Chat.Id;
            var msgId = msg.MessageId;
            var msgText = msg.Text;
            var dateDate = DateTime.UtcNow.ToString("yyyy-MM-dd", new DateTimeFormatInfo());

            var isAdminOrPrivate = await _telegramService.IsAdminOrPrivateChat()
                .ConfigureAwait(false);
            
            if (!isAdminOrPrivate)
            {
                var send = "Maaf, hanya Admin yang dapat mengekspor daftar RSS";
                await _telegramService.SendTextAsync(send).ConfigureAwait(false);
                return;
            }

            var rssSettings = await _rssService.GetRssSettingsAsync()
                .ConfigureAwait(false);
            
            Log.Information($"RssSettings: {rssSettings.ToJson(true)}");

            var listRss = new StringBuilder();
            foreach (var rss in rssSettings)
            {
                listRss.AppendLine(rss.UrlFeed);
            }

            Log.Information($"ListUrl: \n{listRss}");

            var listRssStr = listRss.ToString().Trim();
            var sendText = "Daftar RSS ini tidak terenkripsi, dapat di pulihkan di obrolan mana saja. " +
                           "Tambahkan parameter -e agar daftar RSS terenkripsi.";

            if (msgText.Contains("-e", StringComparison.CurrentCulture))
            {
                Log.Information("List RSS will be encrypted.");
                listRssStr = listRssStr.AesEncrypt();
                sendText = "Daftar RSS ini terenkripsi, hanya dapat di pulihkan di obrolan ini!";
            }

            var filePath = $"{chatId}/rss-feed_{dateDate}_{msgId}.txt";
            await listRssStr.WriteTextAsync(filePath).ConfigureAwait(false);

            var fileSend = BotSettings.PathCache + $"/{filePath}";
            await _telegramService.SendMediaAsync(fileSend, MediaType.LocalDocument, sendText)
                .ConfigureAwait(false);

            fileSend.DeleteFile();
        }
    }
}