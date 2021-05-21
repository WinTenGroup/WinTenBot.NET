using System;
using Hangfire;
using Serilog;
using WinTenDev.Zizi.Host.Telegram;
using WinTenDev.Zizi.Host.Tools;

namespace WinTenDev.Zizi.Host.Scheduler
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