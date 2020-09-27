using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Providers;

namespace Zizi.Bot.Services
{
    public class AfkService
    {
        private const string BaseTable = "afk";
        private const string FileJson = "afk.json";

        public async Task<bool> IsExist(string key, string value)
        {
            var data = await new Query(BaseTable)
                .Where(key, value)
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);
            var isExist = data.Any();
            
            Log.Information($"Check AFK Exist: {isExist}");
            
            return isExist;
        }

        public async Task<bool> IsExistInCache(string key, string val)
        {
            var data = await ReadCacheAsync().ConfigureAwait(false);
            var search = data.AsEnumerable()
                .Where(row => row.Field<string>(key) == val);
            if (!search.Any()) return false;

            var filtered = search.CopyToDataTable();
            Log.Information($"AFK found in Caches: {filtered.ToJson(true)}");
            return true;
        }

        public async Task<bool> IsAfkAsync(Message message)
        {
            var fromId = message.From.Id.ToString();
            var chatId = message.Chat.Id.ToString();
            var isAfk = await IsExistInCache("user_id", fromId).ConfigureAwait(false);

            var afkCache = await ReadCacheAsync().ConfigureAwait(false);
            var filteredAfk = afkCache.AsEnumerable()
                .Where(row => row.Field<object>("is_afk").ToBool()
                              && row.Field<string>("chat_id").ToString() == chatId
                              && row.Field<string>("user_id").ToString() == fromId);
            if (!filteredAfk.Any()) isAfk = false;

            Log.Information($"IsAfk: {isAfk}");
            return isAfk;
        }

        public async Task SaveAsync(Dictionary<string, object> data)
        {
            Log.Information(data.ToJson());
            var where = new Dictionary<string, object>()
            {
                {"user_id", data["user_id"]}
            };

            var insert = 0;

            var checkExist = await new Query(BaseTable)
                .Where(where)
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var isExist = checkExist.Any();

            if (isExist)
            {
                insert = await new Query(BaseTable)
                    .Where(where)
                    .ExecForMysql()
                    .UpdateAsync(data)
                    .ConfigureAwait(false);
            }
            else
            {
                insert = await new Query(BaseTable)
                    .ExecForMysql()
                    .InsertAsync(data)
                    .ConfigureAwait(false);
            }

            Log.Information($"SaveAfk: {insert}");
        }

        public async Task<DataTable> GetAllAfk()
        {
            var data = await new Query(BaseTable)
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var dataTable = data.ToJson().ToDataTable();
            return dataTable;
        }

        public async Task UpdateCacheAsync()
        {
            var data = await GetAllAfk()
                .ConfigureAwait(false);
            Log.Information($"Updating AFK caches to {FileJson}");

            await data.WriteCacheAsync(FileJson)
                .ConfigureAwait(false);
        }

        public async Task<DataTable> ReadCacheAsync()
        {
            var dataTable = await FileJson.ReadCacheAsync<DataTable>()
                .ConfigureAwait(false);
            return dataTable;
        }
    }
}