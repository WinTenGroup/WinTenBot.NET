using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using Hangfire;
using Serilog;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Tools
{
    public static class RssFeedUtil
    {
        public static async Task<int> ExecBroadcasterAsync(long chatId)
        {
            Log.Information("Starting RSS Scheduler.");
            int newRssCount = 0;

            var rssService = new RssService();

            Log.Information("Getting RSS settings..");
            var rssSettings = await rssService.GetRssSettingsAsync(chatId);

            var tasks = rssSettings.Select(async rssSetting =>
            {
                // foreach (RssSetting rssSetting in rssSettings)
                // {
                var rssUrl = rssSetting.UrlFeed;

                Log.Information("Processing {RssUrl} for {ChatId}.", rssUrl, chatId);
                try
                {
                    newRssCount += await ExecuteUrlAsync(chatId, rssUrl);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Demystify(), "Broadcasting RSS Feed.");
                    Thread.Sleep(4000);
                }

                // }
            });

            await Task.WhenAll(tasks);

            Log.Information("RSS Scheduler finished. New RSS Count: {NewRssCount}", newRssCount);

            return newRssCount;
        }

        [Obsolete("This function will be moved as Service")]
        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public static async Task<int> ExecuteUrlAsync(long chatId, string rssUrl)
        {
            int newRssCount = 0;
            var rssService = new RssService();

            Log.Information("Reading feed from {0}. Url: {1}", chatId, rssUrl);
            var rssFeeds = await FeedReader.ReadAsync(rssUrl);

            var rssTitle = rssFeeds.Title;

            // var castLimit = 10;
            // var castStep = 0;

            foreach (var rssFeed in rssFeeds.Items)
            {
                // Log.Debug("Rss from url {0} => {1}", rssUrl, rssFeed.ToJson(true));

                // Prevent flood in first time;
                // if (castLimit == castStep)
                // {
                //     Log.Information($"Send stopped due limit {castLimit} for prevent flooding in first time");
                //     break;
                // }

                // var whereHistory = new Dictionary<string, object>()
                // {
                //     ["chat_id"] = chatId,
                //     ["rss_source"] = rssUrl
                // };

                Log.Debug("Getting last history for {0} url {1}", chatId, rssUrl);
                // var rssHistory = await rssService.GetRssHistory(whereHistory);
                var rssHistory = await rssService.GetRssHistory(new RssHistory()
                {
                    ChatId = chatId,
                    RssSource = rssUrl
                });
                var lastRssHistory = rssHistory.LastOrDefault();

                // if (!rssHistory.Any()) break;
                if (rssHistory.Count > 0)
                {
                    Log.Debug("Last send feed {0} => {1}", rssUrl, lastRssHistory.PublishDate);

                    var lastArticleDate = lastRssHistory.PublishDate;
                    var currentArticleDate = rssFeed.PublishingDate;

                    if (currentArticleDate < lastArticleDate)
                    {
                        Log.Information("Current article is older than last article. Stopped.");
                        break;
                    }

                    Log.Debug("LastArticleDate: {0}", lastArticleDate);
                    Log.Debug("CurrentArticleDate: {0}", currentArticleDate);
                }

                Log.Debug("Prepare sending article.");

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

                // var isExist = await rssService.IsExistInHistory(where);
                var isExist = await rssService.IsHistoryExist(new RssHistory()
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
                    await BotSettings.Client.SendTextMessageAsync(chatId, sendText, ParseMode.Html);

                    Log.Debug("Writing to RSS History");
                    // var data = new Dictionary<string, object>()
                    // {
                    //     {"url", rssFeed.Link},
                    //     {"rss_source", rssUrl},
                    //     {"chat_id", chatId},
                    //     {"title", rssFeed.Title},
                    //     {"publish_date", rssFeed.PublishingDate.ToString()},
                    //     {"author", rssFeed.Author},
                    //     {"created_at", DateTime.Now.ToString(CultureInfo.InvariantCulture)}
                    // };
                    //
                    // await rssService.SaveRssHistoryAsync(data);

                    await rssService.SaveRssHistoryAsync(new RssHistory()
                    {
                        Url = rssFeed.Link,
                        RssSource = rssUrl,
                        ChatId = chatId,
                        Title = rssFeed.Title,
                        PublishDate = rssFeed.PublishingDate ?? DateTime.Now,
                        Author = rssFeed.Author ?? "N/A",
                        CreatedAt = DateTime.Now
                    });

                    // castStep++;
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
                        await rssService.DeleteAllByChatId(chatId);

                        UnRegRSS(chatId);
                    }
                }
            }

            return newRssCount;
        }

        public static void UnRegRSS(long chatId)
        {
            var baseId = "rss";
            var reduceChatId = chatId.ToInt64().ReduceChatId();
            var recurringId = $"{baseId}-{reduceChatId}";

            Log.Debug("Deleting RSS Cron {0}", chatId);
            RecurringJob.RemoveIfExists(recurringId);
        }

        public static async Task<string> FindUrlFeed(this string url)
        {
            Log.Information("Scanning {0} ..", url);
            var urls = await FeedReader.GetFeedUrlsFromUrlAsync(url);
            Log.Debug("UrlFeeds: {0}", urls.ToJson());

            string feedUrl = "";
            var urlCount = urls.Count();

            if (urlCount == 1) // no url - probably the url is already the right feed url
                feedUrl = url;
            else if (urlCount == 1)
                feedUrl = urls.First().Url;
            else if (urlCount == 2
            ) // if 2 urls, then its usually a feed and a comments feed, so take the first per default
                feedUrl = urls.First().Url;

            return feedUrl;
        }

        public static async Task<bool> IsValidUrlFeed(this string url)
        {
            bool isValid = false;
            try
            {
                var feed = await FeedReader.ReadAsync(url);
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Demystify(), "Validating RSS Feed");
            }

            Log.Debug("{0} IsValidUrlFeed: {1}", url, isValid);

            return isValid;
        }
    }
}