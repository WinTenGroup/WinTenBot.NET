using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils.Providers;

namespace WinTenDev.Zizi.Host.Telegram
{
    public static class SafeMemberUtil
    {
        private const string TableName = "SafeMembers";

        public static async Task<bool> IsSafeMemberAsync(this TelegramService telegramService)
        {
            var tableName = "SafeMembers";
            var message = telegramService.Message;
            var fromId = message.From.Id;
            bool isSafe = false;

            var query = (await new Query(tableName)
                    .ExecForMysql(true)
                    .Where("UserId", fromId)
                    .GetAsync<SafeMember>())
                .ToList().FirstOrDefault();

            if (query != null)
                isSafe = query.SafeStep >= 10;

            Log.Debug("Is UserId: {0} safe? {1}", fromId, isSafe);

            return isSafe;
        }

        public static async Task<SafeMember> VerifySafeMemberAsync(this TelegramService telegramService)
        {
            var tableName = "SafeMembers";
            var message = telegramService.Message;
            var fromId = message.From.Id;

            var query = (await new Query(tableName)
                    .ExecForMysql(true)
                    .Where("UserId", fromId)
                    .GetAsync<SafeMember>())
                .ToList().FirstOrDefault();

            if (query != null)
            {
                var lastStep = query.SafeStep;

                if (lastStep >= 10)
                {
                    Log.Debug("UserID {0} has marked has SafeMember", fromId);
                    return query;
                }

                if (lastStep == -1)
                {
                    Log.Debug("SafeMember disabled for {0}", fromId);
                    return query;
                }

                Log.Debug("Updating SafeStep for {0}", fromId);
                var updateData = new Dictionary<string, object>
                {
                    {"SafeStep", lastStep + 1},
                    {"UpdatedAt", DateTime.UtcNow}
                };

                var update = await new Query(tableName)
                    .ExecForMysql(true)
                    .Where("UserId", fromId)
                    .UpdateAsync(updateData);
                query.SafeStep++;

                await SyncSafeMemberToCache();
            }
            else
            {
                Log.Debug("Adding new SafeStep for {0}", fromId);
                var insertData = new Dictionary<string, object>
                {
                    {"UserId", fromId},
                    {"SafeStep", 1},
                    {"CreatedAt", DateTime.UtcNow},
                    {"UpdatedAt", DateTime.UtcNow}
                };

                var insert = await new Query(tableName)
                    .ExecForMysql(true)
                    .InsertAsync(insertData);
            }

            Log.Debug("Verify SafeStep finish");
            return query;
        }

        public static async Task SyncSafeMemberToCache()
        {
            var safeClouds = await new Query(TableName)
                .ExecForMysql(true)
                .Where("SafeStep", 10)
                .GetAsync<SafeMember>();

            // await RavenDbProvider.DeleteAll<SafeMember>();
            // RavenDbProvider.Insert(safeClouds);
        }
    }
}