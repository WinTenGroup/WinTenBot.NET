using System;
using Serilog;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Common;
using Zizi.Bot.Services.Features;

namespace Zizi.Bot.Telegram
{
    public static class ChatUtil
    {
        public static long ReduceChatId(this long chatId)
        {
            var chatIdStr = chatId.ToString();
            if (chatIdStr.StartsWith("-100"))
            {
                chatIdStr = chatIdStr[4..];
            }

            Log.Debug("Reduced ChatId from {0} to {1}", chatId, chatIdStr);

            return chatIdStr.ToInt64();
        }

        // [Obsolete("This method will be moved")]
        // public static async Task EnsureChatHealthAsync(this TelegramService telegramService)
        // {
        //     Log.Information("Ensuring chat health..");
        //
        //     var message = telegramService.Message;
        //     var chatId = message.Chat.Id;
        //     var settingsService = new SettingsService
        //     {
        //         Message = message
        //     };
        //
        //     Log.Information("Preparing restore health on chatId {0}..", chatId);
        //     var data = new Dictionary<string, object>()
        //     {
        //         {"chat_id", chatId},
        //         {"chat_title", message.Chat.Title ?? @"N\A"},
        //         {"chat_type", message.Chat.Type.Humanize()},
        //         {"is_admin", telegramService.IsBotAdmin}
        //     };
        //
        //     var saveSettings = await settingsService.SaveSettingsAsync(data);
        //     Log.Debug("Ensure Settings: {0}", saveSettings);
        //
        //     // await settingsService.UpdateCache();
        // }

        [Obsolete("Please use property instead of this Method")]
        public static bool IsPrivateChat(this TelegramService telegramService)
        {
            var messageOrEdited = telegramService.MessageOrEdited;
            var chat = messageOrEdited.Chat;
            var isPrivate = chat.Type == ChatType.Private;

            Log.Debug("Chat {0} IsPrivateChat => {1}", chat.Id, isPrivate);
            return isPrivate;
        }

        [Obsolete("This method will be moved to TelegramService")]
        public static bool IsGroupChat(this TelegramService telegramService)
        {
            var messageOrEdited = telegramService.MessageOrEdited;
            var chat = messageOrEdited.Chat;
            var isGroupChat = chat.Type == ChatType.Group || chat.Type == ChatType.Supergroup;

            Log.Debug("Chat {0} IsGroupChat => {1}", chat.Id, isGroupChat);
            return isGroupChat;
        }

        public static bool IsChannel(this TelegramService telegramService)
        {
            var messageOrEdited = telegramService.MessageOrEdited;
            var chat = messageOrEdited.Chat;
            var isChannel = chat.Type == ChatType.Channel;

            Log.Debug("Chat {0} IsChannel => {1}", chat.Id, isChannel);
            return isChannel;
        }
    }
}