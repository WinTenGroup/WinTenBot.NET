using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using WinTenBot.Common;
using WinTenBot.IO;
using WinTenBot.Model;
using WinTenBot.Providers;

namespace WinTenBot.Services
{
    public class GlobalBanService
    {
        private string fbanTable = "global_bans";
        private const string GBanAdminTable = "gban_admin";

        private string fileJson = "fban_user.json";

        // private MySqlProvider _mySqlProvider;
        private readonly Message _message;

        public GlobalBanService(Message message)
        {
            _message = message;
            // _mySqlProvider = new MySqlProvider();
        }

        public async Task<bool> IsExist(int userId)
        {
            var where = new Dictionary<string, object>()
            {
                {"user_id", userId}
            };
            // return await IsDataExist(fbanTable, where);

            var query = await new Query(fbanTable)
                .ExecForMysql(true)
                .Where(where)
                .GetAsync()
                .ConfigureAwait(false);

            var isBan = query.Any();
            Log.Information($"{userId} IsBan: {isBan}");

            return isBan;
        }

        public async Task<bool> IsExist(GlobalBanData globalBanData)
        {
            var userId = globalBanData.UserId;
            var where = new Dictionary<string, object>()
            {
                {"user_id", userId}
            };

            var query = await new Query(fbanTable)
                .ExecForMysql(true)
                .Where(where)
                .GetAsync()
                .ConfigureAwait(false);

            var isBan = query.Any();
            Log.Information($"{globalBanData} is Banned?: {isBan}");

            return isBan;
        }

        public async Task<bool> IsExistInCache(int userId)
        {
            var data = await ReadCacheAsync()
                .ConfigureAwait(false);
            DataTable filtered = new DataTable(null);

            Log.Information($"Checking {userId} in Global Ban Cache");
            var search = data.AsEnumerable()
                .Where(row => row.Field<string>("user_id") == userId.ToString());
            if (search.Any())
            {
                filtered = search.CopyToDataTable();
            }

            Log.Information($"Caches found: {filtered.ToJson()}");
            return filtered.Rows.Count > 0;
        }

        [Obsolete("Please use GlobalBan model as parameter.")]
        public async Task<bool> SaveBanAsync(Dictionary<string, object> data)
        {
            Log.Information($"Inserting new data for {_message.Chat.Id}");
            var query = await new Query(fbanTable)
                .ExecForMysql(true)
                .InsertAsync(data)
                .ConfigureAwait(false);

            return query > 0;
            // return await Insert(fbanTable, data);
        }

        public async Task<bool> SaveBanAsync(GlobalBanData globalBanData)
        {
            var userId = globalBanData.UserId;
            var fromId = globalBanData.BannedBy;
            var chatId = globalBanData.BannedFrom;
            var reason = globalBanData.ReasonBan;

            var data = new Dictionary<string, object>()
            {
                {"user_id", userId},
                {"from_id", fromId},
                {"chat_id", chatId},
                {"reason", reason}
            };

            Log.Information($"Inserting new GBan: {globalBanData.ToJson(true)}");
            var query = await new Query(fbanTable)
                .ExecForMysql(true)
                .InsertAsync(data)
                .ConfigureAwait(false);

            return query > 0;
        }

        public async Task<bool> DeleteBanAsync(int userId)
        {
            var where = new Dictionary<string, object>() {{"user_id", userId}};
            var delete = await new Query(fbanTable)
                .ExecForMysql(true)
                .Where(where)
                .DeleteAsync()
                .ConfigureAwait(false);

            return delete > 0;
            // return await Delete(fbanTable, where);
        }

        public async Task<DataTable> GetGlobalBanAll()
        {
            var query = await new Query(fbanTable)
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);

            var data = query.ToJson().MapObject<DataTable>();
            // var sql = $"SELECT * FROM {fbanTable}";
            // var data = await _mySqlProvider.ExecQueryAsync(sql);
            return data;
        }

        public async Task UpdateCacheAsync()
        {
            var data = await GetGlobalBanAll()
                .ConfigureAwait(false);

            Log.Information($"Updating Global Ban caches to {fileJson}");
            await data.WriteCacheAsync(fileJson)
                .ConfigureAwait(false);
        }

        public async Task<DataTable> ReadCacheAsync()
        {
            var dataTable = await fileJson.ReadCacheAsync<DataTable>()
                .ConfigureAwait(false);
            return dataTable;
        }

        public async Task RegisterAdminAsync(GBanAdminItem gBanAdminItem)
        {
            var querySql = new Query(GBanAdminTable)
                .ExecForMysql()
                .Where("user_id", gBanAdminItem.UserId);

            var get = await querySql.GetAsync().ConfigureAwait(false);
            if (get.Any())
            {
                await querySql.InsertAsync(new Dictionary<string, object>()
                {
                    {"", ""}
                }).ConfigureAwait(false);
            }
        }
    }
}