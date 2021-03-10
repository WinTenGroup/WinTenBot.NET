using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Telegram;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Scheduler
{
    public static class RssScheduler
    {
        public static void InitScheduler()
        {
            Task.Run(async () =>
            {
                Log.Information("Initializing RSS Scheduler.");
                var rssService = new RssService();
                Log.Information("Getting list Chat ID");
                var listChatId = await rssService.GetAllRssSettingsAsync();

                foreach (var rssSetting in listChatId)
                {
                    var chatId = rssSetting.ChatId.ToInt64();
                    var urlFeed = rssSetting.UrlFeed;

                    RegisterFeed(chatId, urlFeed);
                    // RegisterScheduler(chatId);
                }

                Log.Information("Registering RSS Scheduler complete.");
            });
        }

        public static void TriggerById(string id)
        {
            Log.Information("Starting trigger RSS Job by id: {0}", id);
            var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();
            var filterJobs = recurringJobs.Where((dto, i) => dto.Id == id).ToList();

            foreach (var jobId in filterJobs.Select(job => job.Id))
            {
                Log.Debug("Trigger: {0}", jobId);
                RecurringJob.Trigger(jobId);
            }
        }

        [JobDisplayName("RSS for ChatID {0}. URL: {1}")]
        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [Queue("rss-broadcaster")]
        public static void RegisterFeed(long chatId, string urlFeed)
        {
            var reducedChatId = chatId.ReduceChatId();
            var unique = String.GenerateUniqueId(5);

            var baseId = "rss";
            var cronInMinute = 1;
            var recurringId = $"{baseId}-{reducedChatId}-{unique}";

            Log.Information("Creating Jobs for {0} with ID: {1}", chatId, recurringId);

            RecurringJob.RemoveIfExists(recurringId);
            RecurringJob.AddOrUpdate(recurringId, () => RssFeedUtil.ExecuteUrlAsync(chatId, urlFeed), $"*/{cronInMinute} * * * *");
            RecurringJob.Trigger(recurringId);

            Log.Information("Registering RSS Scheduler for {0} complete.", chatId);
        }

        public static void RegisterScheduler(this long chatId)
        {
            Log.Information("Initializing RSS Scheduler.");

            var baseId = "rss";
            var cronInMinute = 5;
            var reduceChatId = chatId.ToInt64().ReduceChatId();
            var recurringId = $"{baseId}-{reduceChatId}";

            Log.Information("Creating Jobs for {0}", chatId);

            RecurringJob.RemoveIfExists(recurringId);
            RecurringJob.AddOrUpdate(recurringId, () => RssFeedUtil.ExecBroadcasterAsync(chatId), $"*/{cronInMinute} * * * *");
            RecurringJob.Trigger(recurringId);

            Log.Information("Registering RSS Scheduler for {0} complete.", chatId);
        }
    }
}