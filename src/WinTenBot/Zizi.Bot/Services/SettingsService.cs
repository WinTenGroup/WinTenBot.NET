using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Bot.Telegram;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Services
{
    public class SettingsService
    {
        private string baseTable = "group_settings";

        [Obsolete("This property will be removed.")]
        public Message Message { get; set; }

        public SettingsService()
        {
        }

        [Obsolete("Next time, this constructor will be removed.")]
        public SettingsService(Message message)
        {
            Message = message;
        }

        public async Task<bool> IsSettingExist(long chatId)
        {
            // var where = new Dictionary<string, object>()
            // {
            //     {"chat_id", chatId}
            // };
            //
            // var data = await new Query(baseTable)
            //     .Where(where)
            //     .ExecForMysql()
            //     .GetAsync()
            //     .ConfigureAwait(false);

            var data = await GetSettingsByGroupCore(chatId);
            var isExist = data != null;

            Log.Debug("Group setting IsExist: {0}", isExist);
            return isExist;
        }

        public async Task<ChatSetting> GetSettingsByGroupCore(long chatId)
        {
            var where = new Dictionary<string, object>()
            {
                {"chat_id", chatId}
            };

            var data = await new Query(baseTable)
                .Where(where)
                .ExecForMysql()
                .FirstOrDefaultAsync<ChatSetting>()
                .ConfigureAwait(false);

            return data;
        }

        [Obsolete("Please use Get by ChatId param 'chatId'")]
        public async Task<ChatSetting> GetSettingByGroup()
        {
            var where = new Dictionary<string, object>()
            {
                {"chat_id", Message.Chat.Id}
            };

            var data = await new Query(baseTable)
                .Where(where)
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var mapped = data.ToJson().MapObject<List<ChatSetting>>();

            return mapped.FirstOrDefault();
        }

        public async Task<ChatSetting> GetSettingByGroup(long chatId)
        {
            Log.Information("Get settings chat {0}", chatId);
            var cacheKey = chatId.ReduceChatId() + "-setting";

            if (!MonkeyCacheUtil.IsCacheExist(cacheKey))
            {
                await UpdateSettingsCache(chatId);

                // var mapped = data.ToJson().MapObject<List<ChatSetting>>();
            }

            var cached = MonkeyCacheUtil.Get<ChatSetting>(cacheKey);
            return cached;
        }

        public async Task UpdateSettingsCache(long chatId)
        {
            Log.Debug("Updating cache for {0}", chatId);
            var cacheKey = chatId.ReduceChatId() + "-setting";

            var data = await GetSettingsByGroupCore(chatId);

            data.AddCache(cacheKey);
        }

        public async Task<List<CallBackButton>> GetSettingButtonByGroup()
        {
            var where = new Dictionary<string, object>()
            {
                {"chat_id", Message.Chat.Id}
            };

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

            var data = await new Query(baseTable)
                .Select(selectColumns)
                .Where(where)
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);

            // Log.Debug($"PreTranspose: {data.ToJson()}");
            // data.ToJson(true).ToFile("settings_premap.json");

            using var dataTable = data.ToJson().ToDataTable();

            var rowId = dataTable.Rows[0]["id"].ToString();
            Log.Debug($"RowId: {rowId}");

            var transposedTable = dataTable.TransposedTable();
            // Log.Debug($"PostTranspose: {transposedTable.ToJson()}");
            // transposedTable.ToJson(true).ToFile("settings_premap.json");

            // Log.Debug("Setting Buttons:{0}", transposedTable.ToJson());

            var listBtn = new List<CallBackButton>();
            foreach (DataRow row in transposedTable.Rows)
            {
                var textOrig = row["id"].ToString();
                var value = row[rowId].ToString();

                Log.Debug($"Orig: {textOrig}, Value: {value}");

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
            Log.Debug($"ListBtn: {listBtn.ToJson(true)}");
            // listBtn.ToJson(true).ToFile("settings_listbtn.json");

            return listBtn;
        }

        public async Task<int> SaveSettingsAsync(Dictionary<string, object> data)
        {
            var chatId = data["chat_id"].ToInt64();
            var where = new Dictionary<string, object>() { { "chat_id", chatId } };

            Log.Debug("Checking settings for {0}", chatId);

            var isExist = await IsSettingExist(chatId);

            var insert = -1;
            Log.Debug("Group setting IsExist: {0}", isExist);
            if (!isExist)
            {
                Log.Information($"Inserting new data for {chatId}");

                insert = await new Query(baseTable)
                    .ExecForMysql(true)
                    .InsertAsync(data)
                    .ConfigureAwait(false);
            }
            else
            {
                Log.Information("Updating data for {0}", chatId);

                insert = await new Query(baseTable)
                    .Where(where)
                    .ExecForMysql(true)
                    .UpdateAsync(data)
                    .ConfigureAwait(false);
            }

            return insert;
        }

        [Obsolete("Please use with first of chatId param.")]
        public async Task UpdateCell(string key, object value)
        {
            Log.Debug("Updating Chat Settings {0} => {1}", key, value);
            var where = new Dictionary<string, object>() { { "chat_id", Message.Chat.Id } };
            var data = new Dictionary<string, object>() { { key, value } };

            await new Query(baseTable)
                .Where(where)
                .ExecForMysql()
                .UpdateAsync(data)
                .ConfigureAwait(false);

            await UpdateCache().ConfigureAwait(false);
        }

        public async Task UpdateCell(long chatId, string key, object value)
        {
            Log.Debug("Updating Chat Settings {0} => {1}", key, value);
            var where = new Dictionary<string, object>() { { "chat_id", chatId } };
            var data = new Dictionary<string, object>() { { key, value } };

            await new Query(baseTable)
                .Where(where)
                .ExecForMysql()
                .UpdateAsync(data)
                .ConfigureAwait(false);

            await UpdateSettingsCache(chatId);
        }

        [Obsolete("Cache will be configured automatically.")]
        public async Task<ChatSetting> ReadCache()
        {
            if (Message == null) return null;

            var chatId = Message.Chat.Id.ToString();
            var cachePath = Path.Combine(chatId, "settings.json");
            if (!cachePath.IsFileCacheExist())
            {
                await UpdateCache()
                    .ConfigureAwait(false);
            }

            var cache = await cachePath.ReadCacheAsync<ChatSetting>()
                .ConfigureAwait(false);

            return cache ?? new ChatSetting();
        }

        [Obsolete("Cache will be updated automatically.")]
        public async Task UpdateCache()
        {
            var data = await GetSettingByGroup()
                .ConfigureAwait(false);
            await data.WriteCacheAsync($"{Message.Chat.Id}/settings.json")
                .ConfigureAwait(false);
        }
    }
}