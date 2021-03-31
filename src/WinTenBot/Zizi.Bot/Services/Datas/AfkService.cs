using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using EasyCaching.Core;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Core.Utils;

namespace Zizi.Bot.Services.Datas
{
    public class AfkService
    {
        private const string BaseTable = "afk";
        private const string FileJson = "afk.json";
        private const string CacheKey = "afk";
        private readonly QueryFactory _queryFactory;
        private readonly IEasyCachingProvider _cachingProvider;

        public AfkService(
            IEasyCachingProvider cachingProvider,
            QueryFactory queryFactory)
        {
            _cachingProvider = cachingProvider;
            _queryFactory = queryFactory;
        }

        public async Task<bool> IsExistInDb(int userId)
        {
            var user = await GetAfkByIdCore(userId);
            var isExist = user != null;
            Log.Debug("Is UserId: '{0}' AFK? {1}", userId, isExist);

            return isExist;
        }

        [Obsolete("Please use IsExist(int userId)")]
        public async Task<bool> IsExistInCache(string key, string val)
        {
            var data = await ReadCacheAsync();
            var search = data.AsEnumerable()
                .Where(row => row.Field<string>(key) == val);
            if (!search.Any()) return false;

            var filtered = search.CopyToDataTable();
            Log.Information("AFK found in Caches: {V}", filtered.ToJson(true));
            return true;
        }

        [Obsolete("Please use IsExist(int userId)")]
        public async Task<bool> IsAfkAsync(Message message)
        {
            var fromId = message.From.Id.ToString();
            var chatId = message.Chat.Id.ToString();
            var isAfk = await IsExistInCache("user_id", fromId);

            var afkCache = await ReadCacheAsync();
            var filteredAfk = afkCache.AsEnumerable()
                .Where(row => row.Field<object>("is_afk").ToBool()
                              && row.Field<string>("chat_id").ToString() == chatId
                              && row.Field<string>("user_id").ToString() == fromId);
            if (!filteredAfk.Any()) isAfk = false;

            Log.Information("IsAfk: {IsAfk}", isAfk);
            return isAfk;
        }

        public async Task<IEnumerable<Afk>> GetAfkAllCore()
        {
            var data = await _queryFactory.FromTable(BaseTable).GetAsync<Afk>();

            return data;
        }

        public async Task<Afk> GetAfkByIdCore(int userId)
        {
            var data = await _queryFactory.FromTable(BaseTable)
                .Where("user_id", userId)
                .FirstOrDefaultAsync<Afk>();

            return data;
        }

        public async Task<Afk> GetAfkById(int userId)
        {
            var key = CacheKey + $"-{userId}";
            var isCached = await _cachingProvider.ExistsAsync(key);
            if (!isCached)
            {
                await UpdateAfkByIdCacheAsync(userId);
            }

            var cache = await _cachingProvider.GetAsync<Afk>(key);
            return cache.Value;
        }

        public async Task UpdateAfkByIdCacheAsync(int userId)
        {
            var key = CacheKey + $"-{userId}";
            var afk = await GetAfkByIdCore(userId);
            var timeSpan = TimeSpan.FromMinutes(1);

            Log.Debug("Updating AFK by ID cache with key: '{0}', expire in {1}", key, timeSpan);

            await _cachingProvider.SetAsync(key, afk, timeSpan);
        }

        public async Task SaveAsync(Dictionary<string, object> data)
        {
            Log.Information("Save: {0}", data.ToJson());
            var where = new Dictionary<string, object>()
            {
                {"user_id", data["user_id"]}
            };

            var insert = 0;

            // var checkExist = await _queryFactory.FromTable(BaseTable)
            // .Where(where)
            // .GetAsync<Afk>();

            // var isExist = checkExist.Any();
            var isExist = await IsExistInDb(where["user_id"].ToInt());

            if (isExist)
            {
                insert = await _queryFactory.FromTable(BaseTable)
                    .Where(where)
                    .UpdateAsync(data);
            }
            else
            {
                insert = await _queryFactory.FromTable(BaseTable).InsertAsync(data);
            }

            Log.Information("SaveAfk: {Insert}", insert);
        }

        [Obsolete("Please use method with Return Enumerable AFK.")]
        public async Task<DataTable> GetAllAfk()
        {
            var data = await new Query(BaseTable)
                .ExecForMysql()
                .GetAsync();

            var dataTable = data.ToJson().ToDataTable();
            return dataTable;
        }

        [Obsolete("Cached will be updated automatically")]
        public async Task UpdateCacheAsync()
        {
            var data = await GetAllAfk();
            Log.Information("Updating AFK caches to {FileJson}", FileJson);

            await data.WriteCacheAsync(FileJson);
        }

        public async Task<DataTable> ReadCacheAsync()
        {
            var dataTable = await FileJson.ReadCacheAsync<DataTable>();
            return dataTable;
        }
    }
}