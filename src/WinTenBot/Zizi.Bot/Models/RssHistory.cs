using Newtonsoft.Json;

namespace Zizi.Bot.Models
{
    public class RssHistory
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("chat_id")]
        public string ChatId { get; set; }
        
        [JsonProperty("rss_source")]
        public string RssSource { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("publish_date")]
        public string PublishDate { get; set; }
        
        [JsonProperty("author")]
        public string Author { get; set; }
        
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}