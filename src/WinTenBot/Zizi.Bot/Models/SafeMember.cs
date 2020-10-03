using System;

namespace Zizi.Bot.Models
{
    public class SafeMember
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public int SafeStep { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}