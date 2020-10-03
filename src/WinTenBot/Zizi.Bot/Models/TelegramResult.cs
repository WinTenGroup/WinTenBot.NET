using System;

namespace Zizi.Bot.Models
{
    public class TelegramResult
    {
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
        public string Notes { get; set; }
    }
}