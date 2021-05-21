using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
    public class WarnMemberHistory
    {
        [JsonProperty("from_id")]
        public int FromId { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("step_count")]
        public long StepCount { get; set; }
        
        [JsonProperty("reason_warn")]
        public string ReasonWarn { get; set; }

        [JsonProperty("last_warn_message_id")] 
        public int LastWarnMessageId { get; set; }
        
        [JsonProperty("warner_user_id")]
        public int WarnerUserId { get; set; }
        
        [JsonProperty("warner_first_name")]
        public string WarnerFirstName { get; set; }
        
        [JsonProperty("warner_last_name")]
        public string WarnerLastName { get; set; }

        [JsonProperty("chat_id")]
        public long ChatId { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
}