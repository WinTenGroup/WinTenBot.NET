using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using EasyCaching.Core;
using Serilog;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Telegram;
using Zizi.Core.Utils;

namespace Zizi.Bot.Services.Datas
{
    public class SettingsService
    {
        private const string BaseTable = "group_settings";
        private const string CacheKey = "group-setting";
        private readonly QueryFactory _queryFactory;
        private readonly IEasyCachingProvider _cachingProvider;

        [Obsolete("This property will be removed.")]
        public Message Message { get; set; }

        public SettingsService(
            IEasyCachingProvider cachingProvider,
            QueryFactory queryFactory
        )
        {
            _cachingProvider = cachingProvider;
            _queryFactory = queryFactory;
        }

        [Obsolete("Next time, this constructor will be removed.")]
        public SettingsService(Message message)
        {
            Message = message;
        }

        public async Task<bool> IsSettingExist(long chatId)
        {
            var data = await GetSettingsByGroupCore(chatId);
            var isExist = data != null;

            Log.Debug("Group setting for ChatID '{ChatId}' IsExist? {IsExist}", chatId, isExist);
            return isExist;
        }

        public string GetCacheKey(long chatId)
        {
            return CacheKey + "-" + chatId.ReduceChatId();
        }

        public async Task<ChatSetting> GetSettingsByGroupCore(long chatId)
        {
            var where = new Dictionary<string, object>()
            {
                {"chat_id", chatId}
            };

            var data = await _queryFactory.FromTable(BaseTable)
                .Where(where)
                .FirstOrDefaultAsync<ChatSetting>();

            return data;
        }

        public async Task<ChatSetting> GetSettingsByGroup(long chatId)
        {
            Log.Information("Get settings chat {ChatId}", chatId);
            var cacheKey = GetCacheKey(chatId);

            if (!await _cachingProvider.ExistsAsync(cacheKey))
            {
                await UpdateCacheAsync(chatId);
            }

            var cached = await _cachingProvider.GetAsync<ChatSetting>(cacheKey);

            if (cached.IsNull) return new ChatSetting();

            return cached.Value;
        }

        public async Task UpdateCacheAsync(long chatId)
        {
            Log.Debug("Updating cache for {ChatId}", chatId);
            var cacheKey = GetCacheKey(chatId);

            var data = await GetSettingsByGroupCore(chatId);

            if (data == null)
            {
                Log.Warning("This may first time chat for this ChatId: {ChatId}", chatId);
                return;
            }

            await _cachingProvider.SetAsync(cacheKey, data, TimeSpan.FromMinutes(10));
        }

        public async Task<List<CallBackButton>> GetSettingButtonByGroup(long chatId)
        {
            var selectColumns = new[]
            {
                "id",
                "enable_anti_malfiles",
                "enable_fed_cas_ban",
                "enable_fed_es2_ban",
                "enable_fed_spamwatch",
                // "enable_find_notes",
                "enable_find_tags",
                "enable_human_verification",
                "enable_reply_notification",
                "enable_warn_username",
                "enable_welcome_message",
                // "enable_word_filter_group",
                "enable_word_filter_global",
                "enable_zizi_mata"
            };

            var data = await _queryFactory.FromTable(BaseTable)
                .Select(selectColumns)
                .Where("chat_id", chatId)
                .GetAsync();

            // Log.Debug($"PreTranspose: {data.ToJson()}");
            // data.ToJson(true).ToFile("settings_premap.json");

            using var dataTable = data.ToJson().MapObject<DataTable>();

            var rowId = dataTable.Rows[0]["id"].ToString();
            Log.Debug("RowId: {RowId}", rowId);

            var transposedTable = dataTable.TransposedTable();
            // Log.Debug($"PostTranspose: {transposedTable.ToJson()}");
            // transposedTable.ToJson(true).ToFile("settings_premap.json");

            // Log.Debug("Setting Buttons:{0}", transposedTable.ToJson());

            var listBtn = new List<CallBackButton>();
            foreach (DataRow row in transposedTable.Rows)
            {
                var textOrig = row["id"].ToString();
                var value = row[rowId].ToString();

                Log.Debug("Orig: {TextOrig}, Value: {Value}", textOrig, value);

                var boolVal = value.ToBool();

                var forCallbackData = textOrig;
                var forCaptionText = textOrig;

                if (!boolVal)
                {
                    forCallbackData = textOrig.Replace("enable", "disable");
                }

                if (boolVal)
                {
                    forCaptionText = textOrig.Replace("enable", "✅");
                }
                else
                {
                    forCaptionText = textOrig.Replace("enable", "🚫");
                }

                var btnText = forCaptionText
                    .Replace("enable_", "")
                    .Replace("_", " ");

                // listBtn.Add(new CallBackButton()
                // {
                //     Text = row["id"].ToString(),
                //     Data = row[rowId].ToString()
                // });

                listBtn.Add(new CallBackButton()
                {
                    Text = btnText.ToTitleCase(),
                    Data = $"setting {forCallbackData}"
                });
            }

            //
            // listBtn.Add(new CallBackButton()
            // {
            //     Text = "Enable Word filter Per-Group",
            //     Data = $"setting {mapped.EnableWordFilterGroupWide.ToString()}_word_filter_per_group"
            // });

            // var x =mapped.Cast<CallBackButton>();

            // MatrixHelper.TransposeMatrix<List<ChatSetting>(mapped);
            Log.Debug("ListBtn: {0}", listBtn.ToJson(true));
            // listBtn.ToJson(true).ToFile("settings_listbtn.json");

            return listBtn;
        }

        public async Task<int> SaveSettingsAsync(Dictionary<string, object> data)
        {
            var chatId = data["chat_id"].ToInt64();
            var where = new Dictionary<string, object>() {{"chat_id", chatId}};

            Log.Debug("Checking settings for {ChatId}", chatId);

            var isExist = await IsSettingExist(chatId);

            int insert;
            // Log.Debug("Group setting IsExist: {0}", isExist);
            if (!isExist)
            {
                Log.Information("Inserting new data for {ChatId}", chatId);

                insert = await _queryFactory.FromTable(BaseTable).InsertAsync(data);
            }
            else
            {
                Log.Information("Updating data for {ChatId}", chatId);

                insert = await _queryFactory.FromTable(BaseTable)
                    .Where(where)
                    .UpdateAsync(data);
            }

            await UpdateCacheAsync(chatId);

            return insert;
        }

        public async Task<int> UpdateCell(long chatId, string key, object value)
        {
            Log.Debug("Updating Chat Settings {Key} => {Value}", key, value);
            var where = new Dictionary<string, object>() {{"chat_id", chatId}};
            var data = new Dictionary<string, object>() {{key, value}};

            var save = await _queryFactory.FromTable(BaseTable)
                .Where(where)
                .UpdateAsync(data);

            await UpdateCacheAsync(chatId);

            return save;
        }
    }
}