using System.Collections.Generic;
using System.Threading.Tasks;
using Humanizer;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class ChatUtil
    {
        public static long ReduceChatId(this long chatId)
        {
            var chatIdStr = chatId.ToString();
            if (chatIdStr.StartsWith("-100"))
            {
                chatIdStr = chatIdStr.Substring(4);
            }

            Log.Debug("Reduced ChatId from {0} to {1}", chatId, chatIdStr);

            return chatIdStr.ToInt64();
        }

        public static async Task EnsureChatHealthAsync(this TelegramService telegramService)
        {
            Log.Information("Ensuring chat health..");

            var message = telegramService.Message;
            var chatId = message.Chat.Id;
            var settingsService = new SettingsService
            {
                Message = message
            };

            Log.Information("Preparing restore health on chatId {0}..", chatId);
            var data = new Dictionary<string, object>()
            {
                {"chat_id", chatId},
                {"chat_title", message.Chat.Title ?? @"N\A"},
                {"chat_type", message.Chat.Type.Humanize()},
                {"is_admin", telegramService.IsBotAdmin}
            };

            var saveSettings = await settingsService.SaveSettingsAsync(data)
                .ConfigureAwait(false);
            Log.Debug("Ensure Settings: {0}", saveSettings);

            await settingsService.UpdateCache()
                .ConfigureAwait(false);
        }
    }
}