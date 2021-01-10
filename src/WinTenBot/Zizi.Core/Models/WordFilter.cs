using Newtonsoft.Json;
using System;
using SqlKata;

namespace Zizi.Core.Models
{
    public class WordFilter
    {
        [Column("word")]
        [JsonProperty("word")]
        public string Word { get; set; }

        [Column("deep_filter")]
        [JsonProperty("deep_filter")]
        public bool DeepFilter { get; set; }

        [Column("is_global")]
        [JsonProperty("is_global")]
        public bool IsGlobal { get; set; }

        [Column("from_id")]
        [JsonProperty("from_id")]
        public string FromId { get; set; }

        [Column("chat_id")]
        [JsonProperty("chat_id")]
        public string ChatId { get; set; }

        [Column("created_at")]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}