using System;
using System.Text.Json.Serialization;

namespace Zizi.Bot.Models
{
    public class WordFilter
    {
        [JsonPropertyName("word")]
        public string Word { get; set; }
        
        [JsonPropertyName("deep_filter")]
        public bool DeepFilter { get; set; }
        
        [JsonPropertyName(("is_global"))]
        public bool IsGlobal { get; set; }
        
        [JsonPropertyName("from_id")]
        public string FromId { get; set; }
        
        [JsonPropertyName("chat_id")]
        public string ChatId { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt{get;set;}
    }
}