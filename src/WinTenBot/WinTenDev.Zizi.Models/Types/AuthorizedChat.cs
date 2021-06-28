using System;

namespace WinTenDev.Zizi.Models.Types
{
    public class AuthorizedChat
    {
        public long ChatId { get; set; }
        public long AuthorizedBy { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsBanned { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}