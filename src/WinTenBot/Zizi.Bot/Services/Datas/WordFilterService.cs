using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EasyCaching.Core;
using Serilog;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Models;
using Zizi.Core.Utils;
using Zizi.Core.Utils.Text;

namespace Zizi.Bot.Services.Datas
{
    public class WordFilterService
    {
        private readonly QueryFactory _queryFactory;
        private readonly IEasyCachingProvider _cachingProvider;
        private readonly QueryService _queryService;
        private const string TableName = "word_filter";
        private const string CacheKey = "word-filter";

        public WordFilterService(
            IEasyCachingProvider cachingProvider,
            QueryFactory queryFactory,
            QueryService queryService
        )
        {
            _cachingProvider = cachingProvider;
            _queryFactory = queryFactory;
            _queryService = queryService;
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
            var queryFactory = _queryService.CreateMySqlConnection();
            var wordFilters = await queryFactory.FromTable(TableName).GetAsync<WordFilter>();

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

        public async Task<TelegramResult> IsMustDelete(string words)
        {
            var sw = Stopwatch.StartNew();
            var isShould = false;
            var telegramResult = new TelegramResult();

            if (words == null)
            {
                Log.Information("Scan message skipped because Words is null");
                return telegramResult;
            }

            var listWords = await GetWords();

            var partedWord = words.Split(new[] {'\n', '\r', ' ', '\t'},
                StringSplitOptions.RemoveEmptyEntries);

            Log.Debug("Message Lists: {V}", partedWord.ToJson(true));
            foreach (var word in partedWord)
            {
                var forCompare = word;
                forCompare = forCompare.ToLowerCase().CleanExceptAlphaNumeric();

                foreach (var wordFilter in listWords)
                {
                    var isGlobal = wordFilter.IsGlobal;

                    var forFilter = wordFilter.Word.ToLowerCase();
                    if (forFilter.EndsWith("*", StringComparison.CurrentCulture))
                    {
                        var distinctChar = forCompare.DistinctChar();
                        forFilter = forFilter.CleanExceptAlphaNumeric();
                        isShould = forCompare.Contains(forFilter);
                        Log.Debug("'{ForCompare}' LIKE '{ForFilter}' ? {IsShould}. Global: {IsGlobal}",
                            forCompare, forFilter, isShould, isGlobal);

                        if (!isShould)
                        {
                            isShould = distinctChar.Contains(forFilter);
                            Log.Debug(messageTemplate: "'{DistinctChar}' LIKE '{ForFilter}' ? {IsShould}. Global: {IsGlobal}",
                                distinctChar, forFilter, isShould, isGlobal);
                        }
                    }
                    else
                    {
                        forFilter = wordFilter.Word.ToLowerCase().CleanExceptAlphaNumeric();
                        if (forCompare == forFilter) isShould = true;
                        Log.Debug("'{ForCompare}' == '{ForFilter}' ? {IsShould}, Global: {IsGlobal}",
                            forCompare, forFilter, isShould, isGlobal);
                    }

                    if (!isShould) continue;
                    telegramResult.Notes = $"Filter: {forFilter}, Kata: {forCompare}";
                    telegramResult.IsSuccess = true;
                    Log.Information("Break check now!");
                    break;
                }

                if (!isShould) continue;
                Log.Information("Should break!");
                break;
            }

            Log.Information("Check Message complete in {Elapsed}", sw.Elapsed);

            return telegramResult;
        }
    }
}