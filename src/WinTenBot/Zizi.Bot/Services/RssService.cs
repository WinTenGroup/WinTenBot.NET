using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Services
{
    public class RssService
    {
        private Message _message;
        private string baseTable = "rss_history";
        private string baseTable2 = "RssHistory";
        private string rssSettingTable = "rss_settings";

        public RssService()
        {
        }

        public RssService(Message message = null)
        {
            _message = message;
        }

        [Obsolete("Please use RssHistory as parameter")]
        public async Task<bool> IsExistInHistory(Dictionary<string, object> where)
        {
            var data = await new Query(baseTable)
                .Where(where)
                .ExecForSqLite(true)
                .GetAsync()
                .ConfigureAwait(false);

            var isExist = data.Any();
            Log.Information($"Check RSS History: {isExist}");

            return isExist;
        }

        public async Task<bool> IsExistInHistory(RssHistory rssHistory)
        {
            var where = new Dictionary<string, object>()
            {
                {"ChatId", rssHistory.ChatId},
                {"Url", rssHistory.Url}
            };

            var data = await new Query(baseTable2)
                .Where(where)
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);

            var isExist = data.Any();
            Log.Information($"Check RSS History: {isExist}");

            return isExist;
        }

        public async Task<bool> IsExistRssAsync(string urlFeed)
        {
            var data = await new Query(rssSettingTable)
                .Where("chat_id", _message.Chat.Id)
                .Where("url_feed", urlFeed)
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var isExist = data.Any();
            Log.Information($"Check RSS Setting: {isExist}");

            return isExist;
        }

        public async Task<bool> SaveRssSettingAsync(Dictionary<string, object> data)
        {
            var insert = await new Query(rssSettingTable)
                .ExecForMysql()
                .InsertAsync(data)
                .ConfigureAwait(false);

            return insert.ToBool();
        }

        [Obsolete("Please use RssHistory as parameter")]
        public async Task<bool> SaveRssHistoryAsync(Dictionary<string, object> data)
        {
            var insert = await new Query(baseTable)
                .ExecForSqLite(true)
                .InsertAsync(data)
                .ConfigureAwait(false);

            return insert.ToBool();
        }

        public async Task<int> SaveRssHistoryAsync(RssHistory rssHistory)
        {
            var insert = await new Query(baseTable2)
                .ExecForMysql(true)
                .InsertAsync(rssHistory)
                .ConfigureAwait(false);

            return insert;
        }

        public async Task<List<RssSetting>> GetRssSettingsAsync(long chatId = -1)
        {
            if (chatId == -1)
            {
                chatId = _message.Chat.Id;
            }

            var data = await new Query(rssSettingTable)
                .Where("chat_id", chatId)
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var mapped = data.ToJson().MapObject<List<RssSetting>>();
            Log.Debug("RSSData: {0}", mapped.ToJson(true));

            return mapped;
        }

        public async Task<List<RssSetting>> GetAllRssSettingsAsync()
        {
            var data = await new Query(rssSettingTable)
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var mapped = data.ToJson().MapObject<List<RssSetting>>();
            Log.Debug("RSSData: {0}", mapped.ToJson(true));

            return mapped;
        }

        public async Task<List<RssSetting>> GetListChatIdAsync()
        {
            var data = await new Query(rssSettingTable)
                .Select("chat_id")
                .Distinct()
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var mapped = data.ToJson().MapObject<List<RssSetting>>();

            Log.Information($"Get List ChatID: {data.Count()}");
            return mapped;
        }

        [Obsolete("Please use RssHistory as parameter")]
        public async Task<List<RssHistory>> GetRssHistory(Dictionary<string, object> where)
        {
            var query = await new Query(baseTable)
                .ExecForSqLite()
                .Where(where)
                .GetAsync()
                .ConfigureAwait(false);

            return query.ToJson().MapObject<List<RssHistory>>();
        }

        public async Task<List<RssHistory>> GetRssHistory(RssHistory rssHistory)
        {
            var where = new Dictionary<string, object>()
            {
                ["ChatId"] = rssHistory.ChatId,
                ["RssSource"] = rssHistory.RssSource
            };

            var query = await new Query(baseTable2)
                .ExecForMysql(true)
                .Where(where)
                .GetAsync()
                .ConfigureAwait(false);

            return query.ToJson().MapObject<List<RssHistory>>();
        }

        public async Task<bool> DeleteRssAsync(string urlFeed)
        {
            var delete = await new Query(rssSettingTable)
                .Where("chat_id", _message.Chat.Id)
                .Where("url_feed", urlFeed)
                .ExecForMysql()
                .DeleteAsync()
                .ConfigureAwait(false);

            $"Delete {urlFeed} status: {delete.ToBool()}".ToConsoleStamp();

            return delete.ToBool();
        }

        public async Task<int> DeleteAllByChatId(long chatId)
        {
            var delete = await new Query(rssSettingTable)
                .Where("chat_id", chatId)
                .ExecForMysql()
                .DeleteAsync()
                .ConfigureAwait(false);

            Log.Information("Deleted RSS {0} Settings {1} rows", chatId, delete);

            return delete;
        }

        public async Task DeleteDuplicateAsync()
        {
            var duplicate = await rssSettingTable.MysqlDeleteDuplicateRowAsync("url_feed")
                .ConfigureAwait(false);
            Log.Information($"Delete duplicate on {rssSettingTable} {duplicate}");
        }
    }
}