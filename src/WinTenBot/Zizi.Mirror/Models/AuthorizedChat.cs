using System;
using Serilog;

namespace Zizi.Mirror
{
    public class AuthorizedChat
    {
        public long ChatId { get; set; }
        public int AuthorizedBy { get; set; }
        public bool IsAuthorized { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}