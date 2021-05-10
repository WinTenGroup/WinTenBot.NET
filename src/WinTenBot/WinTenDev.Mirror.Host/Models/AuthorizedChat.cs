using System;
using Serilog;

namespace WinTenDev.Mirror.Host
{
    public class AuthorizedChat
    {
        public long ChatId { get; set; }
        public int AuthorizedBy { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsBanned { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}