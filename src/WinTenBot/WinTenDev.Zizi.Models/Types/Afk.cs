using System;

namespace WinTenDev.Zizi.Models.Types
{
    public class Afk
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long ChatId { get; set; }
        public string AfkReason { get; set; }
        public bool IsAfk { get; set; }
        public DateTime AfkStart { get; set; }
        public DateTime AfkEnd { get; set; }
    }
}