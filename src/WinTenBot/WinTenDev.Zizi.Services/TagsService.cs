using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCaching.Core;
using Serilog;
using SqlKata.Execution;
using Telegram.Bot.Types;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Services
{
    public class TagsService
    {
        private readonly string TableName = "tags";
        private readonly string _jsonCache = "tags.json";
        private readonly QueryFactory _queryFactory;
        private readonly IEasyCachingProvider _cachingProvider;

        public TagsService(
            QueryFactory queryFactory,
            IEasyCachingProvider cachingProvider
        )
        {
            _queryFactory = queryFactory;
            _cachingProvider = cachingProvider;
        }

        public async Task<bool> IsExist(long chatId, string tagVal)
        {
            var data = await GetTagByTag(chatId, tagVal);
            var isExist = data.Any();
            Log.Debug("Tag in '{ChatId}' with slug '{TagVal} is exist? {IsExist}", chatId, tagVal, isExist);

            return isExist;
        }

        public string GetCacheKey(long chatId)
        {
            return $"{chatId.ReduceChatId()}-tags";
        }

        public async Task<IEnumerable<CloudTag>> GetTagsByGroupAsync(long chatId)
        {
            var key = GetCacheKey(chatId);

            if (!await _cachingProvider.ExistsAsync(key))
            {
                await UpdateCacheAsync(chatId);
            }

            var cached = await _cachingProvider.GetAsync<IEnumerable<CloudTag>>(key);

            Log.Debug("Tags for ChatId: {0} => {1}", chatId, cached.ToJson(true));
            return cached.Value;
        }

        public async Task<IEnumerable<CloudTag>> GetTagsByGroupCoreAsync(long chatId)
        {
            var data = await _queryFactory.FromTable(TableName)
                .Where("chat_id", chatId)
                .OrderBy("tag")
                .GetAsync<CloudTag>();

            return data;
        }

        public async Task<IEnumerable<CloudTag>> GetTagByTag(long chatId, string tag)
        {
            var data = await _queryFactory.FromTable(TableName)
                .Where("chat_id", chatId)
                .Where("tag", tag)
                .OrderBy("tag")
                .GetAsync<CloudTag>();

            Log.Debug("Tag by Tag for {0} => {1}", chatId, data.ToJson(true));
            return data;
        }

        public async Task SaveTagAsync(Dictionary<string, object> data)
        {
            var insert = await _queryFactory.FromTable(TableName)
                .InsertAsync(data);

            Log.Information("SaveTag: {Insert}", insert);
        }

        public async Task<bool> DeleteTag(long chatId, string tag)
        {
            var delete = await _queryFactory.FromTable(TableName)
                .Where("chat_id", chatId)
                .Where("tag", tag)
                .DeleteAsync();

            return delete > 0;
        }

        [Obsolete("Please use parameter ChatId")]
        public async Task UpdateCacheAsync(Message message)
        {
            var chatId = message.Chat.Id;
            var data = await GetTagsByGroupAsync(chatId);


            MonkeyCacheUtil.SetChatCache(message, "tags", data);
        }

        public async Task UpdateCacheAsync(long chatId)
        {
            var key = GetCacheKey(chatId);
            var data = await GetTagsByGroupCoreAsync(chatId);

            await _cachingProvider.SetAsync(key, data, TimeSpan.FromMinutes(1));
        }
    }
}