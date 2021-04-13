using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Rss
{
    public class SetRssCommand : CommandBase
    {
        private readonly RssService _rssService;
        private readonly TelegramService _telegramService;

        public SetRssCommand(TelegramService telegramService, RssService rssService)
        {
            _telegramService = telegramService;
            _rssService = rssService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var chatId = _telegramService.Message.Chat.Id;
            var fromId = _telegramService.Message.From.Id;

            var url = _telegramService.Message.Text.GetTextWithoutCmd();
            if (url.IsNullOrEmpty())
            {
                await _telegramService.SendTextAsync("Apa url Feednya?");
                return;
            }

            await _telegramService.AppendTextAsync($"URL: {url}");

            if (!url.CheckUrlValid())
            {
                await _telegramService.AppendTextAsync("Url tersebut sepertinya tidak valid");
                return;
            }

            await _telegramService.AppendTextAsync($"Sedang mengecek apakah berisi RSS");

            var isValid = await url.IsValidUrlFeed();
            if (!isValid)
            {
                await _telegramService.AppendTextAsync("Sedang mencari kemungkinan tautan RSS yang valid");
                var foundUrl = await url.GetBaseUrl().FindUrlFeed();
                Log.Information("Found URL Feed: {FoundUrl}", foundUrl);

                if (foundUrl != "")
                {
                    url = foundUrl;
                }
                else
                {
                    var notfoundRss = $"Kami tidak dapat memvalidasi {url} adalah Link RSS yang valid, " +
                                      $"dan mencoba mencari di {url.GetBaseUrl()} tetap tidak dapat menemukan.";

                    await _telegramService.EditAsync(notfoundRss);
                    return;
                }
            }

            var isFeedExist = await _rssService.IsExistRssAsync(chatId, url);

            Log.Information("Is Url Exist: {IsFeedExist}", isFeedExist);

            if (!isFeedExist)
            {
                await _telegramService.AppendTextAsync($"Sedang menyimpan..");

                var data = new Dictionary<string, object>()
                {
                    {"url_feed", url},
                    {"chat_id", chatId},
                    {"from_id", fromId}
                };

                await _rssService.SaveRssSettingAsync(data);

                await _telegramService.AppendTextAsync("Memastikan Scheduler sudah berjalan");

                var reducedChatId = chatId.ReduceChatId();
                var unique = String.GenerateUniqueId(5);

                var baseId = "rss";
                var recurringId = $"{baseId}-{reducedChatId}-{unique}";
                HangfireUtil.RegisterJob<RssFeedService>(recurringId, service => service.ExecuteUrlAsync(chatId, url), Cron.Minutely, queue: "rss-feed");

                await _telegramService.AppendTextAsync($"Tautan berhasil di simpan");
            }
            else
            {
                await _telegramService.AppendTextAsync($"Tautan sudah di simpan");
            }
        }
    }
}