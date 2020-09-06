﻿namespace WinTenApi.Models
{
    public abstract class TopTrendActivity
    {
        public string ChatId { get; set; }
        public string ChatTitle { get; set; }
        public string ChatUsername { get; set; }
        public long HitCount { get; set; }
    }
}