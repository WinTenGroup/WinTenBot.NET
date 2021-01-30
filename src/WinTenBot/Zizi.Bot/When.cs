using Microsoft.AspNetCore.Http;
using Telegram.Bot.Framework.Abstractions;

namespace Zizi.Bot
{
    public static class When
    {
        public static bool Webhook(IUpdateContext context) =>
            context.Items.ContainsKey(nameof(HttpContext));

        public static bool NewUpdate(IUpdateContext context) =>
            context.Update != null;

        public static bool NewMessage(IUpdateContext context) =>
            context.Update.Message != null;

        public static bool EditedMessage(IUpdateContext context) =>
            context.Update.EditedMessage != null;

        public static bool NewOrEditedMessage(IUpdateContext context) =>
            context.Update.Message != null ||
            context.Update.EditedMessage != null;

        public static bool NewTextMessage(IUpdateContext context) =>
            context.Update.Message?.Text != null ||
            context.Update.EditedMessage?.Text != null;

        public static bool NewCommand(IUpdateContext context)
        {
            var isNewCommand = false;
            if (context.Update.Message != null)
            {
                isNewCommand = context.Update.Message.Text.StartsWith("/");
            }

            if (context.Update.EditedMessage != null)
            {
                isNewCommand = context.Update.EditedMessage.Text.StartsWith("/");
            }

            return isNewCommand;
        // return context.Update.Message?.Entities?.First()?.Type == MessageEntityType.BotCommand;
        }

        public static bool PingReceived(IUpdateContext context) =>
            context.Update.Message?.Text.ToLower() == "ping" ||
            context.Update.Message?.Text.ToLower() == "/ping";

        public static bool CallTagReceived(IUpdateContext context)
        {
            var isTrue = false;
            var message = context.Update.Message ?? context.Update.EditedMessage;

            if (message.Text != null)
            {
                isTrue = message.Text.Contains("#");
            }

            return isTrue;
            // return context.Update.Message.Text.Contains('#');
        }

        public static bool MembersChanged(IUpdateContext context) =>
            context.Update.ChannelPost?.NewChatMembers != null ||
            context.Update.ChannelPost?.LeftChatMember != null;

        public static bool LeftChatMember(IUpdateContext context) =>
            context.Update.Message?.LeftChatMember != null;

        public static bool NewChatMembers(IUpdateContext context) =>
            context.Update.Message?.NewChatMembers != null;

        public static bool NewPinnedMessage(IUpdateContext context) =>
            context.Update.Message?.PinnedMessage != null;

        public static bool LocationMessage(IUpdateContext context) =>
            context.Update.Message?.Location != null;

        public static bool StickerMessage(IUpdateContext context) =>
            context.Update.Message?.Sticker != null;

        public static bool MediaReceived(IUpdateContext context) =>
            context.Update.Message?.Document != null ||
            context.Update.Message?.Photo != null;

        public static bool CallbackQuery(IUpdateContext context) =>
            context.Update.CallbackQuery != null;
    }
}