using System;
using Newtonsoft.Json;

namespace Zizi.Bot.Models
{
   public class HitActivity
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("via_bot")]
        public string ViaBot { get; set; }

        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("from_id")]
        public long FromId { get; set; }

        [JsonProperty("from_first_name")]
        public string FromFirstName { get; set; }

        [JsonProperty("from_last_name")]
        public string FromLastName { get; set; }

        [JsonProperty("from_username")]
        public string FromUsername { get; set; }

        [JsonProperty("from_lang_code")]
        public string FromLangCode { get; set; }

        [JsonProperty("chat_id")]
        public string ChatId { get; set; }

        [JsonProperty("chat_username")]
        public object ChatUsername { get; set; }

        [JsonProperty("chat_type")]
        public string ChatType { get; set; }

        [JsonProperty("chat_title")]
        public string ChatTitle { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}