using System;
using Newtonsoft.Json;
using Zizi.Bot.Enums;

namespace Zizi.Bot.Model
{
    public class CloudTag
    {
        [JsonProperty("chat_id")]
        public string ChatId { get; set; }
        
        [JsonProperty("from_id")]
        public string FromId { get; set; }
        
        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
        
        [JsonProperty("btn_data")]
        public string BtnData { get; set; }
        
        [JsonProperty("type_data")]
        public MediaType TypeData { get; set; }
        
        [JsonProperty("file_id")]
        public string FileId { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}