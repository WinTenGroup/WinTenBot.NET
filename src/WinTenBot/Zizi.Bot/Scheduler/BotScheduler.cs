using System;
using System.IO;
using Hangfire;
using Serilog;
using Zizi.Bot.IO;
using Zizi.Bot.Telegram;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Scheduler
{
    [Obsolete("This register will be moved to HangfireService")]
    static class BotScheduler
    {
        [Obsolete("This register will be moved to HangfireService")]
        public static void StartScheduler()
        {
            // HangfireUtil.DeleteAllJobs();

            // MonkeyCacheRemover();
            // StartLogCleanupScheduler();
            // StartSyncWordFilter();
            // StartCronFlushHitActivity();
            // RssScheduler.InitScheduler();

            // StartSyncGlobalBanToLocal();
        }

        [Obsolete("This register will be moved to HangfireService")]
        private static void StartLogCleanupScheduler()
        {
            var jobId = "logfile-cleanup";
            var path = Path.Combine("Storage", "Logs");

            Log.Debug("Starting cron Log Cleaner with id {JobId}", jobId);
            RecurringJob.RemoveIfExists(jobId);
            // RecurringJob.AddOrUpdate(jobId, () => Storage.ClearLogs(path, "Zizi", true), Cron.Hourly);
            RecurringJob.AddOrUpdate(jobId, () => Storage.ClearLog(), Cron.Hourly);
            RecurringJob.Trigger(jobId);
        }

        [Obsolete("This register will be moved to HangfireService")]
        private static void MonkeyCacheRemover()
        {
            const string jobId = "monkey-cache-remover";
            Log.Debug("Starting cron Monkey Cache Remover. ID: {0}", jobId);
            RecurringJob.RemoveIfExists(jobId);
            RecurringJob.AddOrUpdate(jobId, () => MonkeyCacheUtil.DeleteExpired(), Cron.Daily);
            RecurringJob.Trigger(jobId);
        }

        private static void StartSyncWordFilter()
        {
            const string jobId = "sync-word-filter";

            Log.Debug("Starting cron Sync Word Filter to Local Storage");
            RecurringJob.RemoveIfExists(jobId);
            RecurringJob.AddOrUpdate(jobId, () =>
                SyncUtil.SyncWordToLocalAsync(), Cron.Minutely);
            RecurringJob.Trigger(jobId);
        }

        private static void StartCronFlushHitActivity()
        {
            const string jobId = "flush-hit-activity";

            Log.Debug("Starting cron Flush HitActivity buffer Storage");
            RecurringJob.RemoveIfExists(jobId);
            RecurringJob.AddOrUpdate(jobId, () => Metrics.FlushHitActivity(), Cron.Minutely);
            // RecurringJob.Trigger(jobId);
        }
    }
}