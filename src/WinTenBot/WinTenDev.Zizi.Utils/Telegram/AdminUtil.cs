using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WinTenDev.Zizi.Utils.Telegram
{
    public static class AdminUtil
    {
        private const string BaseCacheKey = "admin";

        private static string GetCacheKey(long chatId)
        {
            var reduced = chatId.ReduceChatId();
            var keyCache = $"{reduced}-{BaseCacheKey}";
            return keyCache;
        }

        public static async Task UpdateCacheAdminAsync(this ITelegramBotClient client, long chatId)
        {
            var keyCache = GetCacheKey(chatId);

            Log.Information("Updating list Admin Cache with key: {0}", keyCache);
            var admins = await client.GetChatAdministratorsAsync(chatId);

            admins.AddCache(keyCache);
        }

        [Obsolete("This method will be moved to TelegramService")]
        public static async Task<ChatMember[]> GetChatAdmin(this ITelegramBotClient botClient, long chatId)
        {
            var keyCache = GetCacheKey(chatId);

            var cacheExist = MonkeyCacheUtil.IsCacheExist(keyCache);
            if (!cacheExist)
            {
                await botClient.UpdateCacheAdminAsync(chatId);
            }

            var chatMembers = MonkeyCacheUtil.Get<ChatMember[]>(keyCache);
            // Log.Debug("ChatMemberAdmin: {0}", chatMembers.ToJson(true));

            return chatMembers;
        }

        public static async Task<bool> IsAdminChat(this ITelegramBotClient botClient, long chatId, int userId)
        {
            var sw = Stopwatch.StartNew();

            var chatMembers = await botClient.GetChatAdmin(chatId);

            var isAdmin = chatMembers.Any(admin => userId == admin.User.Id);

            Log.Debug("Check UserID {V} Admin on Chat {V1}? {V2}. Time: {V3}", userId, chatId, isAdmin, sw.Elapsed);

            sw.Stop();

            return isAdmin;
        }
    }
}