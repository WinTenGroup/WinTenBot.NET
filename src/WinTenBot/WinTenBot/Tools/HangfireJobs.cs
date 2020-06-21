using System;
using System.Data;
using Hangfire;
using Hangfire.MySql.Core;
using Hangfire.Storage;
using Serilog;
using WinTenBot.Model;

namespace WinTenBot.Tools
{
    public static class HangfireJobs
    {
        public static void DeleteAllJobs()
        {
            using var connection = JobStorage.Current.GetConnection();
            foreach (var recurringJob in connection.GetRecurringJobs())
            {
                var recurringJobId = recurringJob.Id;
                Log.Information($"Deleting {recurringJobId}");

                RecurringJob.RemoveIfExists(recurringJobId);
            }
        }

        public static MySqlStorage GetMysqlStorage()
        {
            var connectionString = BotSettings.HangfireMysqlDb;

            var options = new MySqlStorageOptions
            {
                TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
            };
            var storage = new MySqlStorage(connectionString, options);
            return storage;
        }

        // public static SQLiteStorage GetSqliteStorage()
        // {
        //     var connectionString = BotSettings.HangfireSqliteDb;
        //     Log.Information($"HangfireSqlite: {connectionString}");
        //
        //     var options = new SQLiteStorageOptions()
        //     {
        //         QueuePollInterval = TimeSpan.FromSeconds(10)
        //     };
        //
        //     var storage = new SQLiteStorage(connectionString, options);
        //     return storage;
        // }

        // public static LiteDbStorage GetLiteDbStorage()
        // {
        //     var connectionString = BotSettings.HangfireLiteDb;
        //     Log.Information($"HangfireLiteDb: {connectionString}");
        //
        //     var options = new LiteDbStorageOptions()
        //     {
        //         QueuePollInterval = TimeSpan.FromSeconds(10)
        //     };
        //
        //     var storage = new LiteDbStorage(connectionString, options);
        //     return storage;
        // }

        // public static RedisStorage GetRedisStorage()
        // {
        //     var connStr = "127.0.0.1:6379,password=azhe1234";
        //     Log.Information($"Hangfire Redis: {connStr}");
        //     
        //     var options = new RedisStorageOptions()
        //     {
        //         Db = (int)RedisMap.HangfireStorage
        //     };
        //     
        //     var storage = new RedisStorage(connStr,options);
        //     return storage;
        //
        // }
    }
}