using Newtonsoft.Json;
using SqlKata;
using System;

namespace WinTenDev.Zizi.Models.Types
{
    public class WarnUsernameHistory
    {
        [JsonProperty("from_id")]
        [Column("from_id")]
        public long FromId { get; set; }

        [JsonProperty("first_name")]
        [Column("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        [Column("last_name")]
        public string LastName { get; set; }

        [JsonProperty("step_count")]
        [Column("step_count")]
        public long StepCount { get; set; }

        [JsonProperty("last_warn_message_id")]
        [Column("last_warn_message_id")]
        public int LastWarnMessageId { get; set; }

        [JsonProperty("chat_id")]
        [Column("chat_id")]
        public long ChatId { get; set; }

        [JsonProperty("created_at")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}