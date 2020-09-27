using System;
using Newtonsoft.Json;

namespace Zizi.Bot.Model
{
    public class RssSetting
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("chat_id")]
        public string ChatId { get; set; }
        
        [JsonProperty("from_id")]
        public string FromId { get; set; }
        
        [JsonProperty("url_feed")]
        public string UrlFeed { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}