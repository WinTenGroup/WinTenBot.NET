using Telegram.Bot.Types;

namespace Zizi.Bot.Telegram
{
    public static class MemberUtil
    {
        public static string GetNameLink(this int userId, string name)
        {
            return $"<a href='tg://user?id={userId}'>{name}</a>";
        }

        public static string GetFullName(this User user)
        {
            var firstName = user.FirstName;
            var lastName = user.LastName;

            return (firstName + " " + lastName).Trim();
        }

        public static string GetNameLink(this User user)
        {
            var fullName = user.GetFullName();

            return $"<a href='tg://user?id={user.Id}'>{fullName}</a>";
        }

        public static string GetFromNameLink(this Message message)
        {
            var fromId = message.From.Id;
            var fullName = message.From.GetFullName();

            return $"<a href='tg://user?id={fromId}'>{fullName}</a>";
        }
    }
}