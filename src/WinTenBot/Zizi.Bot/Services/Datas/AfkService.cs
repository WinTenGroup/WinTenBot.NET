using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyCaching.Core;
using Serilog;
using SqlKata.Execution;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
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
        private readonly QueryService _queryService;

        public AfkService(
            IEasyCachingProvider cachingProvider,
            QueryFactory queryFactory,
            QueryService queryService
        )
        {
            _cachingProvider = cachingProvider;
            _queryFactory = queryFactory;
            _queryService = queryService;
        }

        public async Task<bool> IsExistInDb(int userId)
        {
            var user = await GetAfkByIdCore(userId);
            var isExist = user != null;
            Log.Debug("Is UserId: '{0}' AFK? {1}", userId, isExist);

            return isExist;
        }

        public async Task<IEnumerable<Afk>> GetAfkAllCore()
        {
            var queryFactory = _queryService.CreateMySqlConnection();
            var data = await queryFactory.FromTable(BaseTable).GetAsync<Afk>();

            return data;
        }

        public async Task<Afk> GetAfkByIdCore(int userId)
        {
            var queryFactory = _queryService.CreateMySqlConnection();
            var data = await queryFactory.FromTable(BaseTable)
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

            var queryFactory = _queryService.CreateMySqlConnection();

            var where = new Dictionary<string, object>()
            {
                {"user_id", data["user_id"]}
            };

            var insert = 0;

            var isExist = await IsExistInDb(where["user_id"].ToInt());

            if (isExist)
            {
                insert = await queryFactory.FromTable(BaseTable)
                    .Where(where)
                    .UpdateAsync(data);
            }
            else
            {
                insert = await queryFactory.FromTable(BaseTable).InsertAsync(data);
            }

            Log.Information("SaveAfk: {Insert}", insert);
        }
    }
}