using Newtonsoft.Json;
using WinTenBot.Enums;

namespace WinTenBot.Model
{
    public class ChatSetting
    {
        [JsonProperty("chat_id")]
        public string ChatId { get; set; }

        [JsonProperty("member_count")]
        public long MemberCount { get; set; }

        [JsonProperty("welcome_message")]
        public string WelcomeMessage { get; set; }

        [JsonProperty("welcome_button")]
        public string WelcomeButton { get; set; }

        [JsonProperty("welcome_media")]
        public string WelcomeMedia { get; set; }

        [JsonProperty("welcome_media_type")]
        public MediaType WelcomeMediaType { get; set; }

        [JsonProperty("rules_text")]
        public string RulesText { get; set; }

        [JsonProperty("last_tags_message_id")]
        public string LastTagsMessageId { get; set; }
        
        [JsonProperty("last_welcome_message_id")]
        public string LastWelcomeMessageId { get; set; }

        [JsonProperty("enable_afk_stat")] 
        public bool EnableAfkStat { get; set; } = true;

        [JsonProperty("enable_global_ban")]
        public bool EnableGlobalBan { get; set; } = true;

        [JsonProperty("enable_human_verification")]
        public bool EnableHumanVerification { get; set; } = true;

        [JsonProperty("enable_fed_cas_ban")] 
        public bool EnableFedCasBan { get; set; } = true;

        [JsonProperty("enable_fed_es2_ban")] 
        public bool EnableFedEs2 { get; set; } = true;

        [JsonProperty("enable_fed_spamwatch")] 
        public bool EnableFedSpamWatch { get; set; } = true;

        [JsonProperty("enable_find_notes")]
        public bool EnableFindNotes { get; set; }

        [JsonProperty("enable_find_tags")] 
        public bool EnableFindTags { get; set; } = true;

        [JsonProperty("enable_word_filter_group")]
        public bool EnableWordFilterPerGroup { get; set; } = true;

        [JsonProperty("enable_word_filter_global")]
        public bool EnableWordFilterGroupWide { get; set; } = true;

        [JsonProperty("enable_warn_username")] 
        public bool EnableWarnUsername { get; set; } = true;

        [JsonProperty("enable_welcome_message")]
        public bool EnableWelcomeMessage { get; set; } = true;

        [JsonProperty("enable_zizi_mata")]
        public bool EnableZiziMata { get; set; } = true;
    }
}