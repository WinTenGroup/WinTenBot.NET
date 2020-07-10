using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using WinTenBot.Common;
using WinTenBot.Model;
using WinTenBot.Providers;
using WinTenBot.Tools;

namespace WinTenBot.Services
{
    public class RssService
    {
        private Message _message;
        private string baseTable = "rss_history";
        private string rssSettingTable = "rss_settings";

        public RssService()
        {
        }

        public RssService(Message message = null)
        {
            _message = message;
        }

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

        public async Task<bool> SaveRssHistoryAsync(Dictionary<string, object> data)
        {
            var insert = await new Query(baseTable)
                .ExecForSqLite(true)
                .InsertAsync(data)
                .ConfigureAwait(false);

            return insert.ToBool();
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
            Log.Debug("RSSData: {0}" , mapped.ToJson(true));

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

        public async Task<List<RssHistory>> GetRssHistory(Dictionary<string, object> where)
        {
            var query = await new Query(baseTable)
                .ExecForSqLite()
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

        public async Task DeleteDuplicateAsync()
        {
            var duplicate = await rssSettingTable.MysqlDeleteDuplicateRowAsync("url_feed")
                .ConfigureAwait(false);
            Log.Information($"Delete duplicate on {rssSettingTable} {duplicate}");
        }
    }
}