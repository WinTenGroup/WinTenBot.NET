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
    public class GlobalBanService
    {
        private const string FbanTable = "global_bans";
        private const string GBanAdminTable = "gban_admin";
        private const string CacheKey = "global-bans";
        private readonly string fileJson = "fban_user.json";

        private readonly Message _message;

        private readonly IEasyCachingProvider _cachingProvider;
        private readonly QueryFactory _queryFactory;

        public GlobalBanService(
            QueryFactory queryFactory,
            IEasyCachingProvider cachingProvider)
        {
            _queryFactory = queryFactory;
            _cachingProvider = cachingProvider;
        }

        public async Task<bool> IsExist(int userId)
        {
            var query = await GetGlobalBanFromDbById(userId);

            var isBan = query != null;
            Log.Information("UserId '{0}' Is ES2 Ban? {1}", userId, isBan);

            return isBan;
        }

        public string GetCacheKey(int userId)
        {
            return $"global-ban_{userId}";
        }

        /// <summary>
        /// Saves the ban async.
        /// </summary>
        /// <param name="globalBanData">The global ban data.</param>
        /// <returns>A Task.</returns>
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

            Log.Information("Inserting new GBan: {0}", globalBanData.ToJson(true));
            var query = await _queryFactory.FromTable(FbanTable)
                .InsertAsync(data);

            return query > 0;
        }

        /// <summary>
        /// Deletes the ban async.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Delete GBan by userId</returns>
        public async Task<bool> DeleteBanAsync(int userId)
        {
            var where = new Dictionary<string, object>() { { "user_id", userId } };
            var delete = await _queryFactory.FromTable(FbanTable)
                .Where(where)
                .DeleteAsync();

            return delete > 0;
            // return await Delete(fbanTable, where);
        }

        /// <summary>
        /// Gets the global ban from db by id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Banned user by userId</returns>
        public async Task<GlobalBanData> GetGlobalBanFromDbById(int userId)
        {
            var where = new Dictionary<string, object>()
            {
                {"user_id", userId}
            };

            var query = await _queryFactory.FromTable(FbanTable)
                .Where(where)
                .FirstOrDefaultAsync<GlobalBanData>();

            return query;
        }

        /// <summary>
        /// Gets the global ban by id (cached).
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A Task.</returns>
        public async Task<GlobalBanData> GetGlobalBanByIdC(int userId)
        {
            var cacheKey = GetCacheKey(userId);
            if (!await _cachingProvider.ExistsAsync(cacheKey))
            {
                var data = await GetGlobalBanFromDbById(userId);

                if (data != null)
                    await _cachingProvider.SetAsync(cacheKey, data, TimeSpan.FromMinutes(10));
            }

            var cache = await _cachingProvider.GetAsync<GlobalBanData>(cacheKey);

            return cache.Value;
        }

        /// <summary>
        /// Gets the global ban from db.
        /// </summary>
        /// <returns>A IEnumerable GlobalBanData</returns>
        public async Task<IEnumerable<GlobalBanData>> GetGlobalBanFromDb()
        {
            var query = await _queryFactory.FromTable(FbanTable).GetAsync<GlobalBanData>();

            return query;
        }

        /// <summary>
        /// Gets the global bans.
        /// </summary>
        /// <returns>Get all Global Bans user</returns>
        public async Task<IEnumerable<GlobalBanData>> GetGlobalBans()
        {
            var cacheKey = "global-bans";
            if (!await _cachingProvider.ExistsAsync(cacheKey))
            {
                var data = await GetGlobalBanFromDb();

                await _cachingProvider.SetAsync(cacheKey, data, TimeSpan.FromMinutes(10));
            }

            var cache = await _cachingProvider.GetAsync<IEnumerable<GlobalBanData>>(cacheKey);

            return cache.Value;
        }

        [Obsolete("Please use GlobalBanData strong typed result")]
        public async Task<DataTable> GetGlobalBanAll()
        {
            var query = await new Query(FbanTable)
                .ExecForMysql(true)
                .GetAsync();

            var data = query.ToJson().MapObject<DataTable>();

            return data;
        }

        public async Task UpdateGBanCache(int userId = -1)
        {
            if (userId == -1)
            {
                var data = await GetGlobalBanFromDb();
                await _cachingProvider.SetAsync(CacheKey, data, TimeSpan.FromMinutes(10));
            }
            else
            {
                var cacheKey = GetCacheKey(userId);
                var data = await GetGlobalBanFromDbById(userId);

                if (data != null)
                    await _cachingProvider.SetAsync(cacheKey, data, TimeSpan.FromMinutes(10));
            }
        }

        #region GBan Admin

        public async Task<bool> IsGBanAdminAsync(long userId)
        {
            var querySql = await _queryFactory.FromTable(GBanAdminTable)
                .Where("user_id", userId)
                .GetAsync<GBanAdminItem>();

            var isRegistered = querySql.Any();
            Log.Debug("UserId {0} is registered on ES2? {1}", userId, isRegistered);


            return isRegistered;
        }

        public async Task RegisterAdminAsync(GBanAdminItem gBanAdminItem)
        {
            var querySql = _queryFactory.FromTable(GBanAdminTable).Where("user_id", gBanAdminItem.UserId);

            var get = await querySql.GetAsync();

            if (get.Any())
            {
                await querySql.InsertAsync(new Dictionary<string, object>()
                {
                    {"", ""}
                });
            }
        }

        public async Task SaveAdminGban(GBanAdminItem adminItem)
        {
            var querySql = await _queryFactory.FromTable(GBanAdminTable)
                .InsertAsync(adminItem);
            Log.Debug("Insert GBanReg: {0}", querySql);
        }


        #endregion
    }
}