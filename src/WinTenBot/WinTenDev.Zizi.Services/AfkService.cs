using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyCaching.Core;
using Serilog;
using SqlKata.Execution;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Services
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

        /// <summary>Determines whether AFK is exist by User ID</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        ///     true if AFK exist otherwise, false.
        /// </returns>
        public async Task<bool> IsExistCore(int userId)
        {
            var user = await GetAfkByIdCore(userId);
            var isExist = user != null;
            Log.Debug("Is UserId: '{UserId}' AFK? {IsExist}", userId, isExist);

            return isExist;
        }

        /// <summary>Gets the afk all core.</summary>
        /// <returns>
        ///   Return All AFK bot-wide (un-cached)
        /// </returns>
        public async Task<IEnumerable<Afk>> GetAfkAllCore()
        {
            var queryFactory = _queryService.CreateMySqlConnection();
            var data = await queryFactory.FromTable(BaseTable).GetAsync<Afk>();

            return data;
        }

        /// <summary>Gets the afk by identifier core.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        ///   Return single AFK row by User ID (un-cached)
        /// </returns>
        public async Task<Afk> GetAfkByIdCore(int userId)
        {
            var queryFactory = _queryService.CreateMySqlConnection();
            var data = await queryFactory.FromTable(BaseTable)
                .Where("user_id", userId)
                .FirstOrDefaultAsync<Afk>();

            return data;
        }

        /// <summary>Gets the afk by identifier.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Return AFK by User ID (cached)</returns>
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

        /// <summary>Updates the AFK Cache by User ID</summary>
        /// <param name="userId">The user identifier.</param>
        public async Task UpdateAfkByIdCacheAsync(int userId)
        {
            var key = CacheKey + $"-{userId}";
            var afk = await GetAfkByIdCore(userId);
            var timeSpan = TimeSpan.FromMinutes(1);

            Log.Debug("Updating AFK by ID cache with key: '{Key}', expire in {TimeSpan}", key, timeSpan);

            if (afk == null)
            {
                Log.Warning("Caching AFK UserID {UserId} disabled beacuse AFK Data is null", userId);
                return;
            }

            await _cachingProvider.SetAsync(key, afk, timeSpan);
        }

        /// <summary>Save AFK data to Database</summary>
        /// <param name="data">The dictionary of AFK data.</param>
        public async Task SaveAsync(Dictionary<string, object> data)
        {
            Log.Information("Save: {V}", data.ToJson());

            var queryFactory = _queryService.CreateMySqlConnection();

            var where = new Dictionary<string, object>()
            {
                {"user_id", data["user_id"]}
            };

            int saveResult;

            var isExist = await IsExistCore(@where["user_id"].ToInt());

            if (isExist)
            {
                saveResult = await queryFactory.FromTable(BaseTable)
                    .Where(where)
                    .UpdateAsync(data);
            }
            else
            {
                saveResult = await queryFactory.FromTable(BaseTable).InsertAsync(data);
            }

            Log.Information("SaveAfk: {Insert}", saveResult);
        }
    }
}