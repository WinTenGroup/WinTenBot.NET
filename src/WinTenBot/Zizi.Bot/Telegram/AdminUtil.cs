using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Telegram
{
    public static class AdminUtil
    {
        private const string CacheKey = "admin-group";

        public static async Task UpdateCacheAdminAsync(this TelegramService telegramService)
        {
            var client = telegramService.Client;
            var message = telegramService.Message;
            var chatId = message.Chat.Id;
            var admins = await client.GetChatAdministratorsAsync(chatId)
                .ConfigureAwait(false);

            telegramService.SetChatCache(CacheKey, admins);
        }
        
        public static async Task<ChatMember[]> GetChatAdmin(this TelegramService telegramService)
        {
            var message = telegramService.Message;
            // var client = telegramService.Client;
            // var chatId = message.Chat.Id;

            var cacheExist = telegramService.IsChatCacheExist(CacheKey);
            if (!cacheExist)
            {
                await telegramService.UpdateCacheAdminAsync().ConfigureAwait(false);
                // var admins = await client.GetChatAdministratorsAsync(chatId)
                    // .ConfigureAwait(false);

                // telegramService.SetChatCache(CacheKey, admins);
            }

            var chatMembers = telegramService.GetChatCache<ChatMember[]>(CacheKey);
            // Log.Debug("ChatMemberAdmin: {0}", chatMembers.ToJson(true));

            return chatMembers;
        }

        public static async Task<bool> IsAdminChat(this TelegramService telegramService, int userId = -1)
        {
            var sw = Stopwatch.StartNew();
            var isAdmin = false;
            var message = telegramService.Message;
            userId = userId == -1 ? message.From.Id : userId;

            if (telegramService.IsPrivateChat()) return false;

            var chatMembers = await telegramService.GetChatAdmin()
                .ConfigureAwait(false);

            foreach (var admin in chatMembers)
            {
                if (userId == admin.User.Id)
                {
                    isAdmin = true;
                    break;
                }
            }

            Log.Information("UserId {0} IsAdmin: {1}. Time: {2}", userId, isAdmin, sw.Elapsed);
            sw.Stop();

            return isAdmin;
        }
    }
}