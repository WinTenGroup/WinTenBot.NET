using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class Privilege
    {
        [Obsolete("This method will be moved to TelegramService")]
        public static bool IsSudoer(this int userId)
        {
            bool isSudoer = false;
            var sudoers = BotSettings.Sudoers;
            var match = sudoers.FirstOrDefault(x => x == userId.ToString());

            if (match != null)
            {
                isSudoer = true;
            }

            Log.Information("UserId: {0} IsSudoer: {1}", userId, isSudoer);
            return isSudoer;
        }

        public static bool IsSudoer(this TelegramService telegramService)
        {
            bool isSudoer = false;
            var userId = telegramService.Message.From.Id;
            var sudoers = BotSettings.Sudoers;
            var match = sudoers.FirstOrDefault(x => x == userId.ToString());

            if (match != null)
            {
                isSudoer = true;
            }

            Log.Information("UserId: {0} IsSudoer: {1}", userId, isSudoer);
            return isSudoer;
        }

        public static async Task<bool> IsAdminOrPrivateChat(this TelegramService telegramService)
        {
            var isAdmin = await IsAdminGroup(telegramService);
            var isPrivateChat = telegramService.IsPrivateChat();

            return isAdmin || isPrivateChat;
        }

        public static async Task<bool> IsAdminGroup(this TelegramService telegramService, int userId = -1)
        {
            var message = telegramService.MessageOrEdited;
            var client = telegramService.Client;

            var chatId = message.Chat.Id;
            var fromId = message.From.Id;
            var isAdmin = false;

            if (telegramService.IsPrivateChat()) return false;
            if (userId >= 0) fromId = userId;

            var admins = await client.GetChatAdministratorsAsync(chatId);
            foreach (var admin in admins)
            {
                if (fromId == admin.User.Id)
                {
                    isAdmin = true;
                }
            }

            Log.Information("UserId {0} IsAdmin: {1}", fromId, isAdmin);

            return isAdmin;
        }

        public static async Task<ChatMember[]> GetAllAdmins(this TelegramService telegramService)
        {
            var client = telegramService.Client;
            var message = telegramService.Message;
            var chatId = message.Chat.Id;

            var allAdmins = await client.GetChatAdministratorsAsync(chatId);
            if (BotSettings.IsDevelopment)
                Log.Debug("All Admin on {0} {1}", chatId, allAdmins.ToJson(true));

            return allAdmins;
        }
    }
}