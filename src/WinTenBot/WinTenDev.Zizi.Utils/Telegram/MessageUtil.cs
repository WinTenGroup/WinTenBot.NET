using System;
using System.Linq;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WinTenDev.Zizi.Utils.Telegram
{
    public static class MessageUtil
    {
        public static string GetFileId(this Message message)
        {
            var fileId = "";
            switch (message.Type)
            {
                case MessageType.Document:
                    fileId = message.Document.FileId;
                    break;

                case MessageType.Photo:
                    fileId = message.Photo.Last().FileId;
                    break;

                case MessageType.Video:
                    fileId = message.Video.FileId;
                    break;
            }

            return fileId;
        }

        public static string GetReducedFileId(this Message message)
        {
            return GetFileId(message).Substring(0, 17);
        }

        public static string GetTextWithoutCmd(this string message, bool withoutCmd = true)
        {
            var partsMsg = message.Split(' ');
            var text = message;
            if (withoutCmd && message.StartsWith("/", StringComparison.CurrentCulture))
            {
                text = message.TrimStart(partsMsg[0].ToCharArray());
            }

            return text.Trim();
        }

        public static string GetMessageLink(this Message message)
        {
            var chatUsername = message.Chat.Username;
            var messageId = message.MessageId;

            var messageLink = $"https://t.me/{chatUsername}/{messageId}";

            if (chatUsername.IsNullOrEmpty())
            {
                var trimmedChatId = message.Chat.Id.ToString().Substring(4);
                messageLink = $"https://t.me/c/{trimmedChatId}/{messageId}";
            }

            Log.Debug("MessageLink: {0}", messageLink);
            return messageLink;
        }
    }
}