using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EasyCaching.Core;
using Flurl.Http;
using Serilog;
using SqlKata.Execution;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Services
{
    public class BlockListService
    {
        private readonly IEasyCachingProvider _cachingProvider;
        private readonly QueryFactory _queryFactory;

        public BlockListService(
            IEasyCachingProvider cachingProvider,
            QueryFactory queryFactory
        )
        {
            _cachingProvider = cachingProvider;
            _queryFactory = queryFactory;
        }

        public async Task SaveBlockList(BlockList data)
        {
            await _queryFactory.FromTable("").InsertAsync(data);
        }

        public async Task<BlockListData> ParseList(string url)
        {
            var sw = Stopwatch.StartNew();

            if (!await _cachingProvider.ExistsAsync(url))
            {
                await UpdateCache(url);
            }

            var cache = await _cachingProvider.GetAsync<string>(url);
            var data = cache.Value;

            var parseList = ParseListCore(data);

            Log.Debug("Read BlockList completed in {Elapsed}", sw.Elapsed);
            sw.Stop();

            return parseList;
        }

        public async Task UpdateCache(string url)
        {
            var allUrl = await url.GetStringAsync();
            await _cachingProvider.SetAsync(url, allUrl, TimeSpan.FromHours(1));
        }

        public BlockListData ParseListCore(string rawUrl)
        {
            var lines = rawUrl.Split(Environment.NewLine);

            if (lines.Length == 0) return null;

            var data = new BlockListData();
            var metaData = new Dictionary<string, object>();

            foreach (var line in lines)
            {
                if (!line.Contains("#")) continue;

                var split = line.Split(": ");
                var key = split[0]
                    .Replace("#", "")
                    .Trim()
                    .Replace(" ", "")
                    .Trim();

                metaData.Add(key, split[1].Trim());
            }


            var name = lines[0];
            var source = lines[1];
            var lastUpdate = lines[2];

            var listUrl = lines.Skip(3).Where(s => !s.Trim().IsNullOrEmpty()).ToList();


            data.Name = name;
            data.Source = source;
            data.LastUpdate = lastUpdate;
            data.MetadataDic = metaData;
            data.MetaData = metaData.DictionaryMapper<BlockListMetaData>();
            data.ListDomain = listUrl;
            data.DomainCount = listUrl.Count;

            return data;
        }
    }
}