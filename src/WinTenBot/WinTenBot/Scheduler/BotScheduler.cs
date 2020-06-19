﻿using System.IO;
using Hangfire;
using Serilog;
using WinTenBot.IO;
using WinTenBot.Tools;

namespace WinTenBot.Scheduler
{
    static class BotScheduler
    {
        public static void StartScheduler()
        {
            Tools.Hangfire.DeleteAllJobs();
            
            StartLogCleanupScheduler();
            StartSyncWordFilter();
            RssScheduler.InitScheduler();
            
            // StartSyncGlobalBanToLocal();
        }

        private static void StartLogCleanupScheduler()
        {
            var jobId = "logfile-cleanup";
            var path = Path.Combine("Storage", "Logs");

            Log.Debug($"Starting cron Log Cleaner with id {jobId}");
            RecurringJob.RemoveIfExists(jobId);
            // RecurringJob.AddOrUpdate(jobId, () => Storage.ClearLogs(path, "Zizi", true), Cron.Hourly);
            RecurringJob.AddOrUpdate(jobId, () => Storage.ClearLog(), Cron.Hourly);
            RecurringJob.Trigger(jobId);
        }

        private static void StartSyncWordFilter()
        {
            const string jobId = "sync-word-filter";

            Log.Debug("Starting cron Sync Word Filter to Local Storage");
            RecurringJob.RemoveIfExists(jobId);
            RecurringJob.AddOrUpdate(jobId, () =>
                Sync.SyncWordToLocalAsync(), Cron.Minutely);
            RecurringJob.Trigger(jobId);
        }
    }
}