using System;

namespace WinTenBot.Model
{
    public class TelegramResult
    {
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
        public string Notes { get; set; }
    }
}