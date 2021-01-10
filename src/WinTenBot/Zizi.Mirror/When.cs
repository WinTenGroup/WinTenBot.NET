using Telegram.Bot.Framework.Abstractions;

namespace Zizi.Mirror
{
    public static class When
    {
        public static bool NewUpdate(IUpdateContext context) =>
            context.Update != null;

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
    }
}