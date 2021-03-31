using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCaching.Core;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Core.Utils;

namespace Zizi.Bot.Services.Datas
{
    public class WordFilterService
    {
        private readonly QueryFactory _queryFactory;
        private readonly IEasyCachingProvider _cachingProvider;
        private const string TableName = "word_filter";
        private const string CacheKey = "word-filter";
        private Message Message { get; set; }

        public WordFilterService(
            IEasyCachingProvider cachingProvider,
            QueryFactory queryFactory
        )
        {
            _cachingProvider = cachingProvider;
            _queryFactory = queryFactory;
        }

        public WordFilterService(Message message)
        {
            Message = message;
        }

        public async Task<bool> IsExistAsync(Dictionary<string, object> where)
        {
            var check = await _queryFactory.FromTable(TableName)
                .Where(where)
                .GetAsync();

            var isExist = check.Any();

            Log.Debug("Group setting IsExist: {IsExist}", isExist);

            return isExist;
        }

        public async Task<bool> SaveWordAsync(WordFilter wordFilter)
        {
            Log.Debug("Saving Word to Database");

            var insert = await _queryFactory.FromTable(TableName).InsertAsync(wordFilter);

            return insert > 0;
        }

        public async Task<IEnumerable<WordFilter>> GetAllWordsOnCloud()
        {
            Log.Debug("Getting Words from Database");
            var wordFilters = await _queryFactory.FromTable(TableName).GetAsync<WordFilter>();

            return wordFilters;
        }

        public async Task<IEnumerable<WordFilter>> GetWords()
        {
            Log.Debug("Getting Words from Database");
            if (!await _cachingProvider.ExistsAsync(CacheKey)) await UpdateWordsCache();

            var cache = _cachingProvider.Get<IEnumerable<WordFilter>>(CacheKey);

            return cache.Value;
        }

        public async Task UpdateWordsCache()
        {
            var data = await GetAllWordsOnCloud();

            Log.Debug("Updating Cache Words with key {0}", CacheKey);

            await _cachingProvider.SetAsync(CacheKey, data, TimeSpan.FromMinutes(1));
        }
    }
}