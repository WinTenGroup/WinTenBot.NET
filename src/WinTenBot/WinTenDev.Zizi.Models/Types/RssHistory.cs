﻿using System;
using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
    public class RssHistory
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("chat_id")]
        public long ChatId { get; set; }
        
        [JsonProperty("rss_source")]
        public string RssSource { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("publish_date")]
        public DateTime? PublishDate { get; set; }
        
        [JsonProperty("author")]
        public string Author { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        // [JsonProperty("updated_at")]
        // public string UpdatedAt { get; set; }
    }
}