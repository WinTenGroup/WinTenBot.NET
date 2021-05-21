using Dapper.FluentMap.Mapping;
using Newtonsoft.Json;
using SqlKata;
using WinTenDev.Zizi.Models.Enums;

namespace WinTenDev.Zizi.Models.Types
{
    public class ChatSetting
    {
        [Column("chat_id")]
        [JsonProperty("chat_id")]
        public string ChatId { get; set; }

        [Column("member_count")]
        [JsonProperty("member_count")]
        public long MemberCount { get; set; }

        [Column("event_log_chat_id")]
        [JsonProperty("event_log_chat_id")]
        public long EventLogChatId { get; set; }

        [Column("is_admin")]
        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        [Column("welcome_message")]
        [JsonProperty("welcome_message")]
        public string WelcomeMessage { get; set; }

        [Column("welcome_button")]
        [JsonProperty("welcome_button")]
        public string WelcomeButton { get; set; }

        [Column("welcome_media")]
        [JsonProperty("welcome_media")]
        public string WelcomeMedia { get; set; }

        [Column("welcome_media_type")]
        [JsonProperty("welcome_media_type")]
        public MediaType WelcomeMediaType { get; set; } = MediaType.Unknown;

        [Column("rules_text")]
        [JsonProperty("rules_text")]
        public string RulesText { get; set; }

        [Column("last_tags_message_id")]
        [JsonProperty("last_tags_message_id")]
        public string LastTagsMessageId { get; set; }

        [Column("last_warn_username_message_id")]
        [JsonProperty("last_warn_username_message_id")]
        public string LastWarnUsernameMessageId { get; set; }

        [Column("last_welcome_message_id")]
        [JsonProperty("last_welcome_message_id")]
        public string LastWelcomeMessageId { get; set; }

        [Column("enable_afk_stat")]
        [JsonProperty("enable_afk_stat")]
        public bool EnableAfkStat { get; set; } = true;

        [Column("enable_global_ban")]
        [JsonProperty("enable_global_ban")]
        public bool EnableGlobalBan { get; set; } = true;

        [Column("enable_human_verification")]
        [JsonProperty("enable_human_verification")]
        public bool EnableHumanVerification { get; set; } = true;

        [JsonProperty("enable_fed_cas_ban")]
        [Column("enable_fed_cas_ban")]
        public bool EnableFedCasBan { get; set; } = true;

        [Column("enable_fed_es2_ban")]
        [JsonProperty("enable_fed_es2_ban")]
        public bool EnableFedEs2 { get; set; } = true;

        [Column("enable_fed_spamwatch")]
        [JsonProperty("enable_fed_spamwatch")]
        public bool EnableFedSpamWatch { get; set; } = true;

        [Column("enable_find_notes")]
        [JsonProperty("enable_find_notes")]
        public bool EnableFindNotes { get; set; }

        [Column("enable_find_tags")]
        [JsonProperty("enable_find_tags")]
        public bool EnableFindTags { get; set; } = true;

        [Column("enable_word_filter_group")]
        [JsonProperty("enable_word_filter_group")]
        public bool EnableWordFilterPerGroup { get; set; } = true;

        [Column("enable_word_filter_global")]
        [JsonProperty("enable_word_filter_global")]
        public bool EnableWordFilterGroupWide { get; set; } = true;

        [Column("enable_warn_username")]
        [JsonProperty("enable_warn_username")]
        public bool EnableWarnUsername { get; set; } = true;

        [Column("enable_welcome_message")]
        [JsonProperty("enable_welcome_message")]
        public bool EnableWelcomeMessage { get; set; } = true;

        [Column("enable_zizi_mata")]
        [JsonProperty("enable_zizi_mata")]
        public bool EnableZiziMata { get; set; } = true;
    }

    public class ChatSettingMap : EntityMap<ChatSetting>
    {
        public ChatSettingMap()
        {
            Map(p => p.EnableFedEs2).ToColumn("enable_fed_es2_ban");
            Map(p => p.EnableWordFilterPerGroup).ToColumn("enable_word_filter_group");
            Map(p => p.EnableWordFilterGroupWide).ToColumn("enable_word_filter_global");
        }
    }
}