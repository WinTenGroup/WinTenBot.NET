using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Zizi.Bot.Model;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class EventLog
    {
        public static async Task SendEventAsync(this TelegramService telegramService, string text = "N/A")
        {
            Log.Information("Sending Event to Global and Local..");
            var globalLogTarget = BotSettings.BotChannelLogs;
            var currentSetting = telegramService.CurrentSetting;
            var chatLogTarget = currentSetting.EventLogChatId;

            var listLogTarget = new List<long>();

            if (globalLogTarget != -1) listLogTarget.Add(globalLogTarget);
            if (chatLogTarget != 0) listLogTarget.Add(chatLogTarget);

            foreach (var chatId in listLogTarget)
            {
                await telegramService.SendEventCoreAsync(text, chatId, true)
                    .ConfigureAwait(false);
            }
        }

        public static async Task SendEventCoreAsync(this TelegramService telegramService, string additionalText = "N/A",
            long customChatId = 0, bool disableWebPreview = false)
        {
            var message = telegramService.MessageOrEdited;
            var chatTitle = message.Chat.Title ?? message.From.FirstName;
            var chatId = message.Chat.Id;
            var fromId = message.From.Id;
            var fromNameLink = message.From.GetNameLink();
            var msgLink = message.GetMessageLink();

            var sendLog = "🐾 <b>EventLog Preview</b>" +
                          $"\nGroup: <code>{chatId}</code> - {chatTitle}" +
                          $"\nFrom: <code>{fromId}</code> - {fromNameLink}" +
                          $"\n<a href='{msgLink}'>Go to Message</a>" +
                          $"\nNote: {additionalText}" +
                          $"\n\n#{message.Type} => #ID{chatId.ToString().TrimStart('-')}";

            await telegramService
                .SendTextAsync(sendLog, customChatId: customChatId, disableWebPreview: disableWebPreview)
                .ConfigureAwait(false);
        }
    }
}