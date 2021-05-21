using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata.Execution;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Services
{
    public class RssService
    {
        private readonly string rssHistoryTable = "RssHistory";
        private readonly string rssSettingTable = "rss_settings";
        private readonly string CacheKey = "rss-list";

        private readonly QueryFactory _queryFactory;
        private readonly AppConfig _appConfig;

        public RssService()
        {
        }

        public RssService(
            QueryFactory queryFactory,
            AppConfig appConfig
        )
        {
            _appConfig = appConfig;
            _queryFactory = queryFactory;
        }

        public async Task<bool> IsHistoryExist(RssHistory rssHistory)
        {
            var where = new Dictionary<string, object>()
            {
                {"ChatId", rssHistory.ChatId},
                {"Url", rssHistory.Url}
            };

            var data = await _queryFactory.FromTable(rssHistoryTable)
                .Where(where)
                .GetAsync();

            var isExist = data.Any();
            Log.Debug("Check RSS History: {IsExist}", isExist);

            return isExist;
        }

        public async Task<bool> IsExistRssAsync(long chatId, string urlFeed)
        {
            var data = await _queryFactory.FromTable(rssSettingTable)
                .Where("chat_id", chatId)
                .Where("url_feed", urlFeed)
                .GetAsync();

            var isExist = data.Any();
            Log.Information("Check RSS Setting: {IsExist}", isExist);

            return isExist;
        }

        public string GetCacheKey(long chatId)
        {
            var reduced = chatId.ReduceChatId();
            return $"{CacheKey}_{reduced}";
        }

        public async Task<bool> SaveRssSettingAsync(Dictionary<string, object> data)
        {
            var insert = await _queryFactory.FromTable(rssSettingTable).InsertAsync(data);

            return insert.ToBool();
        }

        public async Task<int> SaveRssHistoryAsync(RssHistory rssHistory)
        {
            var insert = await _queryFactory.FromTable(rssHistoryTable).InsertAsync(rssHistory);

            return insert;
        }

        public async Task<List<RssSetting>> GetRssSettingsAsync(long chatId)
        {
            var data = await _queryFactory.FromTable(rssSettingTable)
                .Where("chat_id", chatId)
                .GetAsync();

            var mapped = data.ToJson().MapObject<List<RssSetting>>();
            Log.Debug("RSSData: {0}", mapped.ToJson(true));

            return mapped;
        }

        public async Task<IEnumerable<RssSetting>> GetAllRssSettingsAsync()
        {
            var data = await _queryFactory.FromTable(rssSettingTable).GetAsync<RssSetting>();
            // Log.Debug("RSSData: {0}", data.ToJson(true));

            return data;
        }

        public async Task<List<RssHistory>> GetRssHistory(RssHistory rssHistory)
        {
            var where = new Dictionary<string, object>()
            {
                ["ChatId"] = rssHistory.ChatId,
                ["RssSource"] = rssHistory.RssSource
            };

            var query = await _queryFactory.FromTable(rssHistoryTable)
                .Where(where)
                .GetAsync();

            return query.ToJson().MapObject<List<RssHistory>>();
        }

        public async Task<bool> DeleteRssAsync(long chatId, string urlFeed)
        {
            var delete = await _queryFactory.FromTable(rssSettingTable)
                .Where("chat_id", chatId)
                .Where("url_feed", urlFeed)
                .DeleteAsync();

            Log.Information("Delete {UrlFeed} status: {V}", urlFeed, delete.ToBool());

            return delete.ToBool();
        }

        public async Task<int> DeleteAllByChatId(long chatId)
        {
            var delete = await _queryFactory.FromTable(rssSettingTable)
                .Where("chat_id", chatId)
                .DeleteAsync();

            Log.Information("Deleted RSS {0} Settings {1} rows", chatId, delete);

            return delete;
        }

        public async Task DeleteDuplicateAsync()
        {
            // var duplicate = await rssSettingTable.MysqlDeleteDuplicateRowAsync("url_feed");
            // Log.Information("Delete duplicate on {RssSettingTable} {Duplicate}", rssSettingTable, duplicate);
        }
    }
}