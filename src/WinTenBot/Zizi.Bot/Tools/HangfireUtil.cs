using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.LiteDB;
using Hangfire.MySql;
using Hangfire.Redis;
using Hangfire.Storage;
using Hangfire.Storage.SQLite;
using Serilog;
using Zizi.Bot.Enums;

namespace Zizi.Bot.Tools
{
    public static class HangfireUtil
    {
        public static void DeleteAllJobs()
        {
            var sw = Stopwatch.StartNew();

            Log.Information("Deleting previous Hangfire jobs..");
            var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();

            var numOfJobs = recurringJobs.Count;

            Parallel.ForEach(recurringJobs, (dto, pls, index) =>
            {
                var recurringJobId = dto.Id;

                Log.Debug("Deleting jobId: {RecurringJobId}, Index: {Index}", recurringJobId, index);
                RecurringJob.RemoveIfExists(recurringJobId);

                Log.Debug("Delete succeeded {RecurringJobId}, Index: {Index}", recurringJobId, index);
            });

            Log.Information("Hangfire jobs successfully deleted. Total: {NumOfJobs}. Time: {Elapsed}", numOfJobs, sw.Elapsed);

            sw.Stop();
        }

        public static void DeleteJob(string jobId)
        {
            Log.Debug("Deleting job by ID: '{JobId}'", jobId);
            RecurringJob.RemoveIfExists(jobId);
            Log.Debug("Job '{JobId}' deleted successfully..", jobId);
        }

        public static MySqlStorage GetMysqlStorage(string connectionStr)
        {
            var options = new MySqlStorageOptions
            {
                // TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
                TablesPrefix = "_hangfire"
            };
            var storage = new MySqlStorage(connectionStr, options);
            return storage;
        }

        public static void RegisterJob(string jobId, Expression<Action> methodCall, Func<string> cronExpression,
            TimeZoneInfo timeZone = null, string queue = "default")
        {
            var sw = Stopwatch.StartNew();

            Log.Debug("Registering Job with ID: {JobId}", jobId);
            RecurringJob.RemoveIfExists(jobId);
            RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression, timeZone, queue);
            RecurringJob.Trigger(jobId);

            Log.Debug("Registering Job {JobId} finish in {Elapsed}", jobId, sw.Elapsed);

            sw.Stop();
        }

        public static void RegisterJob<T>(string jobId, Expression<Func<T, Task>> methodCall, Func<string> cronExpression,
            TimeZoneInfo timeZone = null, string queue = "default")
        {
            var sw = Stopwatch.StartNew();

            Log.Debug("Registering Job with ID: {JobId}", jobId);
            RecurringJob.RemoveIfExists(jobId);
            RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression, timeZone, queue);
            RecurringJob.Trigger(jobId);

            Log.Debug("Registering Job {JobId} finish in {Elapsed}", jobId, sw.Elapsed);

            sw.Stop();
        }

        public static void RegisterJob(string jobId, Expression<Func<Task>> methodCall, Func<string> cronExpression,
            TimeZoneInfo timeZone = null, string queue = "default")
        {
            var sw = Stopwatch.StartNew();

            Log.Debug("Registering Job with ID: {JobId}", jobId);
            RecurringJob.RemoveIfExists(jobId);
            RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression, timeZone, queue);
            RecurringJob.Trigger(jobId);

            Log.Debug("Registering Job {JobId} finish in {Elapsed}", jobId, sw.Elapsed);

            sw.Stop();
        }

        public static int TriggerJobs(string prefixId)
        {
            var sw = Stopwatch.StartNew();

            Log.Information("Loading Hangfire jobs..");
            var connection = JobStorage.Current.GetConnection();

            var recurringJobs = connection.GetRecurringJobs();
            var filteredJobs = recurringJobs.Where(dto => dto.Id.StartsWith(prefixId)).ToList();
            Log.Debug("Found {Count} of {Count1}", filteredJobs.Count, recurringJobs.Count);

            var numOfJobs = filteredJobs.Count;

            Parallel.ForEach(filteredJobs, (dto, pls, index) =>
            {
                var recurringJobId = dto.Id;

                Log.Debug("Triggering jobId: {RecurringJobId}, Index: {Index}", recurringJobId, index);
                RecurringJob.Trigger(recurringJobId);

                Log.Debug("Trigger succeeded {RecurringJobId}, Index: {Index}", recurringJobId, index);
            });

            Log.Information("Hangfire jobs successfully trigger. Total: {NumOfJobs}. Time: {Elapsed}", numOfJobs, sw.Elapsed);

            sw.Stop();

            return filteredJobs.Count;
        }

        public static SQLiteStorage GetSqliteStorage(string connectionString)
        {
            Log.Information("HangfireSqlite: {ConnectionString}", connectionString);

            var options = new SQLiteStorageOptions()
            {
                QueuePollInterval = TimeSpan.FromSeconds(10)
            };

            var storage = new SQLiteStorage(connectionString, options);
            return storage;
        }

        public static LiteDbStorage GetLiteDbStorage(string connectionString)
        {
            Log.Information("HangfireLiteDb: {ConnectionString}", connectionString);

            var options = new LiteDbStorageOptions()
            {
                QueuePollInterval = TimeSpan.FromSeconds(10)
            };

            var storage = new LiteDbStorage(connectionString, options);
            return storage;
        }

        public static RedisStorage GetRedisStorage(string connStr)
        {
            Log.Information("Hangfire Redis: {ConnStr}", connStr);

            var options = new RedisStorageOptions()
            {
                Db = (int) RedisMap.HangfireStorage
            };

            var storage = new RedisStorage(connStr, options);
            return storage;
        }
    }
}