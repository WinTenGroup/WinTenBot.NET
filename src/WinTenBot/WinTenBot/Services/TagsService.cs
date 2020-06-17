using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using WinTenBot.Common;
using WinTenBot.Interfaces;
using WinTenBot.IO;
using WinTenBot.Model;
using WinTenBot.Providers;

namespace WinTenBot.Services
{
    public class TagsService : ITagsService
    {
        private string baseTable = "tags";
        private string jsonCache = "tags.json";

        public async Task<bool> IsExist(long chatId, string tagVal)
        {
            var data = await GetTagByTag(chatId, tagVal)
                .ConfigureAwait(false);
            return data.Count > 0;
        }

        public async Task<List<CloudTag>> GetTagsAsync()
        {
            var query = await new Query("tags")
                .GetAsync()
                .ConfigureAwait(false);

            var mapped = query.ToJson().MapObject<List<CloudTag>>();
            return mapped;

            // var data = await _mySqlProvider.ExecQueryAsync("SELECT * FROM tags");
            // return data;
        }

        public async Task<List<CloudTag>> GetTagsByGroupAsync(string column, long chatId)
        {
            var query = await new Query("tags")
                .Where("chat_id", chatId)
                .OrderBy("tag")
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var mapped = query.ToJson().MapObject<List<CloudTag>>();

            Log.Debug(mapped.ToJson(true));
            return mapped;

            // Log.Debug($"tags: {query.ToJson(true)}");

            // var sql = $"SELECT {column} FROM tags WHERE id_chat = '{chatId}' ORDER BY tag";
            // var data = await _mySqlProvider.ExecQueryAsync(sql);
            // return data;
        }

        public async Task<List<CloudTag>> GetTagByTag(long chatId, string tag)
        {
            var query = await new Query("tags")
                .Where("chat_id", chatId)
                .Where("tag", tag)
                .OrderBy("tag")
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);

            var mapped = query.ToJson().MapObject<List<CloudTag>>();

            Log.Debug(mapped.ToJson(true));
            return mapped;

            // var sql = $"SELECT * FROM tags WHERE id_chat = '{chatId}' AND tag = '{tag}' ORDER BY tag";
            // var data = await _mySqlProvider.ExecQueryAsync(sql);
            // return data;
        }

        public async Task SaveTagAsync(Dictionary<string, object> data)
        {
            var insert = await new Query("tags")
                .ExecForMysql(true)
                .InsertAsync(data)
                .ConfigureAwait(false);
            
            Log.Information($"SaveTag: {insert}");
        }

        public async Task<bool> DeleteTag(long chatId, string tag)
        {
            var delete = await new Query("tags")
                .ExecForMysql()
                .Where("chat_id",chatId)
                .Where("tag",tag)
                .DeleteAsync()
                .ConfigureAwait(false);
            
            // var sql = $"DELETE FROM {baseTable} WHERE id_chat = '{chatId}' AND tag = '{tag}'";
            // var delete = await _mySqlProvider.ExecNonQueryAsync(sql);
            return delete > 0;
        }

        public async Task UpdateCacheAsync(Message message)
        {
            var chatId = message.Chat.Id;
            var data = await GetTagsByGroupAsync("*", chatId)
                .ConfigureAwait(false);
            data.BackgroundWriteCache($"{chatId}/{jsonCache}");
        }
    }
}