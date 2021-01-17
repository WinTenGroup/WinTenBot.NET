using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Tools;
using Zizi.Bot.Interfaces;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Services
{
    public class TagsService
    {
        private string baseTable = "tags";
        private string jsonCache = "tags.json";
        private QueryFactory _queryFactory;

        public TagsService(QueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

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

        public async Task<IEnumerable<CloudTag>> GetTagsByGroupAsync(long chatId)
        {
            var key = $"{chatId.ReduceChatId()}-tags";

            if (!MonkeyCacheUtil.IsCacheExist(key))
            {
                var mapped = await _queryFactory.FromQuery(new Query("tags"))
                    .Where("chat_id", chatId)
                    .OrderBy("tag")
                    .GetAsync<CloudTag>()
                    .ConfigureAwait(false);

                mapped.AddCache(key);
            }

            var cached = MonkeyCacheUtil.Get<IEnumerable<CloudTag>>(key);

            Log.Debug("Tags for ChatId: {0} => {1}", chatId, cached.ToJson(true));
            return cached;

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

            // var mapped = (await LiteDbProvider.GetCollectionsAsync<CloudTag>()
            //         .ConfigureAwait(false))
            //     .Find(x => x.ChatId == chatId.ToString() && x.Tag == tag).ToList();

            Log.Debug("Tag by Tag for {0} => {1}", chatId, mapped.ToJson(true));
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
                .Where("chat_id", chatId)
                .Where("tag", tag)
                .DeleteAsync()
                .ConfigureAwait(false);

            // var sql = $"DELETE FROM {baseTable} WHERE id_chat = '{chatId}' AND tag = '{tag}'";
            // var delete = await _mySqlProvider.ExecNonQueryAsync(sql);
            return delete > 0;
        }

        [Obsolete("Tags cache will be updated automatically.")]
        public async Task UpdateCacheAsync(Message message)
        {
            var chatId = message.Chat.Id;
            var data = await GetTagsByGroupAsync(chatId).ConfigureAwait(false);

            // data.BackgroundWriteCache($"{chatId}/{jsonCache}");

            // var liteDb = await LiteDbProvider.GetCollectionsAsync<CloudTag>()
            // .ConfigureAwait(false);
            // data.ForEach(y => liteDb.DeleteMany(x => x.ChatId == y.ChatId));

            // var insertBulk = liteDb.InsertBulk(data);

            message.SetChatCache("tags", data);
        }
    }
}