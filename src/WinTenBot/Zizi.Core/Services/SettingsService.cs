using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using Telegram.Bot.Types;
using Zizi.Core.Utils;

namespace Zizi.Core.Services
{
    public class SettingsService
    {
        private string baseTable = "group_settings";
        public Message Message { get; set; }

        public SettingsService()
        {
        }

        public SettingsService(Message message)
        {
            Message = message;
        }

        // public async Task<bool> IsSettingExist()
        // {
        //     var where = new Dictionary<string, object>() {{"chat_id", Message.Chat.Id}};
        //
        //     var data = await new Query(baseTable)
        //         .Where(where)
        //         .ExecForMysql()
        //         .GetAsync()
        //         ;
        //     var isExist = data.Any();
        //
        //     Log.Information($"Group setting IsExist: {isExist}");
        //     return isExist;
        // }
        //
        // public async Task<ChatSetting> GetSettingByGroup()
        // {
        //     var where = new Dictionary<string, object>()
        //     {
        //         {"chat_id", Message.Chat.Id}
        //     };
        //
        //     var data = await new Query(baseTable)
        //         .Where(where)
        //         .ExecForMysql()
        //         .GetAsync()
        //         ;
        //
        //     var mapped = data.ToJson().MapObject<List<ChatSetting>>();
        //
        //     return mapped.FirstOrDefault();
        // }

        // public async Task<List<CallBackButton>> GetSettingButtonByGroup()
        // {
        //     var where = new Dictionary<string, object>()
        //     {
        //         {"chat_id", Message.Chat.Id}
        //     };
        //
        //     var selectColumns = new[]
        //     {
        //         "id",
        //         "enable_anti_malfiles",
        //         "enable_fed_cas_ban",
        //         "enable_fed_es2_ban",
        //         "enable_fed_spamwatch",
        //         // "enable_find_notes",
        //         "enable_find_tags",
        //         "enable_human_verification",
        //         "enable_reply_notification",
        //         "enable_warn_username",
        //         "enable_welcome_message",
        //         // "enable_word_filter_group",
        //         "enable_word_filter_global",
        //         "enable_zizi_mata"
        //     };
        //
        //     var data = await new Query(baseTable)
        //         .Select(selectColumns)
        //         .Where(where)
        //         .ExecForMysql(true)
        //         .GetAsync()
        //         ;
        //
        //     // Log.Debug($"PreTranspose: {data.ToJson()}");
        //     // data.ToJson(true).ToFile("settings_premap.json");
        //
        //     using var dataTable = data.ToJson().ToDataTable();
        //
        //     var rowId = dataTable.Rows[0]["id"].ToString();
        //     Log.Debug($"RowId: {rowId}");
        //
        //     var transposedTable = dataTable.TransposedTable();
        //     // Log.Debug($"PostTranspose: {transposedTable.ToJson()}");
        //     // transposedTable.ToJson(true).ToFile("settings_premap.json");
        //
        //     // Log.Debug("Setting Buttons:{0}", transposedTable.ToJson());
        //
        //     var listBtn = new List<CallBackButton>();
        //     foreach (DataRow row in transposedTable.Rows)
        //     {
        //         var textOrig = row["id"].ToString();
        //         var value = row[rowId].ToString();
        //
        //         Log.Debug($"Orig: {textOrig}, Value: {value}");
        //
        //         var boolVal = value.ToBool();
        //
        //         var forCallbackData = textOrig;
        //         var forCaptionText = textOrig;
        //
        //         if (!boolVal)
        //         {
        //             forCallbackData = textOrig.Replace("enable", "disable");
        //         }
        //
        //         if (boolVal)
        //         {
        //             forCaptionText = textOrig.Replace("enable", "✅");
        //         }
        //         else
        //         {
        //             forCaptionText = textOrig.Replace("enable", "🚫");
        //         }
        //
        //         var btnText = forCaptionText
        //             .Replace("enable_", "")
        //             .Replace("_", " ");
        //
        //         // listBtn.Add(new CallBackButton()
        //         // {
        //         //     Text = row["id"].ToString(),
        //         //     Data = row[rowId].ToString()
        //         // });
        //
        //         listBtn.Add(new CallBackButton()
        //         {
        //             Text = btnText.ToTitleCase(),
        //             Data = $"setting {forCallbackData}"
        //         });
        //     }
        //
        //     //
        //     // listBtn.Add(new CallBackButton()
        //     // {
        //     //     Text = "Enable Word filter Per-Group",
        //     //     Data = $"setting {mapped.EnableWordFilterGroupWide.ToString()}_word_filter_per_group"
        //     // });
        //
        //     // var x =mapped.Cast<CallBackButton>();
        //
        //     // MatrixHelper.TransposeMatrix<List<ChatSetting>(mapped);
        //     Log.Debug($"ListBtn: {listBtn.ToJson(true)}");
        //     // listBtn.ToJson(true).ToFile("settings_listbtn.json");
        //
        //     return listBtn;
        // }
        //
        // public async Task<int> SaveSettingsAsync(Dictionary<string, object> data)
        // {
        //     var chatId = data["chat_id"];
        //     var where = new Dictionary<string, object>() {{"chat_id", chatId}};
        //
        //     Log.Debug($"Checking settings for {chatId}");
        //     var check = await new Query(baseTable)
        //         .Where(where)
        //         .ExecForMysql()
        //         .GetAsync()
        //         ;
        //
        //     var isExist = check.Any();
        //
        //     var insert = -1;
        //     Log.Debug($"Group setting IsExist: {isExist}");
        //     if (!isExist)
        //     {
        //         Log.Information($"Inserting new data for {chatId}");
        //
        //         insert = await new Query(baseTable)
        //             .ExecForMysql(true)
        //             .InsertAsync(data)
        //             ;
        //     }
        //     else
        //     {
        //         Log.Information($"Updating data for {chatId}");
        //
        //         insert = await new Query(baseTable)
        //             .Where(where)
        //             .ExecForMysql(true)
        //             .UpdateAsync(data)
        //             ;
        //     }
        //
        //     return insert;
        // }
        //
        // public async Task UpdateCell(string key, object value)
        // {
        //     Log.Debug("Updating Chat Settings {0} => {1}", key, value);
        //     var where = new Dictionary<string, object>() {{"chat_id", Message.Chat.Id}};
        //     var data = new Dictionary<string, object>() {{key, value}};
        //
        //     await new Query(baseTable)
        //         .Where(where)
        //         .ExecForMysql()
        //         .UpdateAsync(data)
        //         ;
        //
        //     await UpdateCache();
        // }
        //
        // public async Task<ChatSetting> ReadCache()
        // {
        //     if (Message == null) return null;
        //
        //     var chatId = Message.Chat.Id.ToString();
        //     var cachePath = Path.Combine(chatId, "settings.json");
        //     if (!cachePath.IsFileCacheExist())
        //     {
        //         await UpdateCache()
        //             ;
        //     }
        //
        //     var cache = await cachePath.ReadCacheAsync<ChatSetting>()
        //         ;
        //
        //     return cache ?? new ChatSetting();
        // }
        //
        // public async Task UpdateCache()
        // {
        //     var data = await GetSettingByGroup()
        //         ;
        //     await data.WriteCacheAsync($"{Message.Chat.Id}/settings.json")
        //         ;
        // }
    }
}