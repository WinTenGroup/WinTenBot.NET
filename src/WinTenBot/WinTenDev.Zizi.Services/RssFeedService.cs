using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using Hangfire;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Services
{
    public class RssFeedService
    {
        private readonly RssService _rssService;
        private readonly TelegramBotClient _botClient;

        public RssFeedService(
            TelegramBotClient botClient,
            RssService rssService
        )
        {
            _botClient = botClient;
            _rssService = rssService;
        }

        public async Task RegisterScheduler()
        {
            Log.Information("Initializing RSS Scheduler..");

            Log.Information("Getting list Chat ID");
            var listChatId = await _rssService.GetAllRssSettingsAsync();

            foreach (var rssSetting in listChatId)
            {
                var chatId = rssSetting.ChatId.ToInt64();
                var urlFeed = rssSetting.UrlFeed;

                var reducedChatId = chatId.ReduceChatId();
                var unique = StringUtil.GenerateUniqueId(5);

                var baseId = "rss";
                var cronInMinute = 1;
                var recurringId = $"{baseId}-{reducedChatId}-{unique}";

                HangfireUtil.RegisterJob<RssFeedService>(recurringId, service => service.ExecuteUrlAsync(chatId, urlFeed), Cron.Minutely);

                // RegisterFeed(chatId, urlFeed);
                // RegisterScheduler(chatId);
            }

            Log.Information("Registering RSS Scheduler complete..");
        }

        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("rss-feed")]
        public async Task<int> ExecuteUrlAsync(long chatId, string rssUrl)
        {
            int newRssCount = 0;

            Log.Information("Reading feed from {0}. Url: {1}", chatId, rssUrl);
            var rssFeeds = await FeedReader.ReadAsync(rssUrl);

            var rssTitle = rssFeeds.Title;

            foreach (var rssFeed in rssFeeds.Items)
            {
                Log.Debug("Getting last history for {0} url {1}", chatId, rssUrl);

                var rssHistory = await _rssService.GetRssHistory(new RssHistory()
                {
                    ChatId = chatId,
                    RssSource = rssUrl
                });
                var lastRssHistory = rssHistory.LastOrDefault();

                if (rssHistory.Count > 0)
                {
                    Log.Debug("Last send feed {0} => {1}", rssUrl, lastRssHistory.PublishDate);

                    var lastArticleDate = lastRssHistory.PublishDate;
                    var currentArticleDate = rssFeed.PublishingDate;

                    if (currentArticleDate < lastArticleDate)
                    {
                        Log.Information("Current article is older than last article. Stopped..");
                        break;
                    }

                    Log.Debug("LastArticleDate: {0}", lastArticleDate);
                    Log.Debug("CurrentArticleDate: {0}", currentArticleDate);
                }

                Log.Debug("Prepare sending article..");

                var titleLink = $"{rssTitle} - {rssFeed.Title}".MkUrl(rssFeed.Link);
                var category = rssFeed.Categories.MkJoin(", ");
                var sendText = $"{rssTitle} - {rssFeed.Title}" +
                               $"\n{rssFeed.Link}" +
                               $"\nTags: {category}";

                var where = new Dictionary<string, object>()
                {
                    {"chat_id", chatId},
                    {"url", rssFeed.Link}
                };

                var isExist = await _rssService.IsHistoryExist(new RssHistory()
                {
                    ChatId = chatId,
                    Url = rssFeed.Link
                });

                if (isExist)
                {
                    Log.Information("Last article from feed '{0}' has sent to {1}", rssUrl, chatId);
                    break;
                }

                Log.Information("Sending article from feed {0} to {1}", rssUrl, chatId);

                try
                {
                    await _botClient.SendTextMessageAsync(chatId, sendText, ParseMode.Html);

                    Log.Debug("Writing to RSS History");

                    await _rssService.SaveRssHistoryAsync(new RssHistory()
                    {
                        Url = rssFeed.Link,
                        RssSource = rssUrl,
                        ChatId = chatId,
                        Title = rssFeed.Title,
                        PublishDate = rssFeed.PublishingDate ?? DateTime.Now,
                        Author = rssFeed.Author ?? "N/A",
                        CreatedAt = DateTime.Now
                    });

                    newRssCount++;
                }
                catch (ChatNotFoundException chatNotFoundException)
                {
                    Log.Information("May Bot not added in {0}.", chatId);
                    Log.Error(chatNotFoundException.Demystify(), "Chat Not Found");
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Demystify(), "RSS Broadcaster error");
                    var exMessage = ex.Message;
                    if (exMessage.Contains("bot was blocked by the user"))
                    {
                        Log.Warning("Seem need clearing all RSS Settings and unregister Cron completely!");
                        Log.Debug("Deleting all RSS Settings");
                        await _rssService.DeleteAllByChatId(chatId);

                        UnRegRSS(chatId);
                    }
                }
            }

            return newRssCount;
        }

        public static void UnRegRSS(long chatId)
        {
            var baseId = "rss";
            var reduceChatId = ChatUtil.ReduceChatId(chatId.ToInt64());
            var recurringId = $"{baseId}-{reduceChatId}";

            Log.Debug("Deleting RSS Cron {0}", chatId);
            RecurringJob.RemoveIfExists(recurringId);
        }
    }
}