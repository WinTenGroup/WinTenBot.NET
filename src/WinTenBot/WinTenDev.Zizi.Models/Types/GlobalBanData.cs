using System;
using Newtonsoft.Json;
using SqlKata;

namespace WinTenDev.Zizi.Models.Types
{
    public class GBanAdminItem
    {
        [Ignore]
        [JsonProperty("id")]
        public long Id { get; set; }

        [Column("user_id")]
        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [Column("username")]
        [JsonProperty("username")]
        public string Username { get; set; }

        [Column("promoted_by")]
        [JsonProperty("promoted_by")]
        public long PromotedBy { get; set; }

        [Column("promoted_from")]
        [JsonProperty("promoted_from")]
        public long PromotedFrom { get; set; }

        [Column("is_banned")]
        [JsonProperty("is_banned")]
        public bool IsBanned { get; set; }

        [Column("created_at")]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class GlobalBanData
    {
        [Column("id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        [Column("user_id")]
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [Column("reason_ban")]
        [JsonProperty("reason_ban")]
        public string ReasonBan { get; set; }

        [Column("banned_by")]
        [JsonProperty("banned_by")]
        public long BannedBy { get; set; }

        [Column("banned_from")]
        [JsonProperty("banned_from")]
        public long BannedFrom { get; set; }

        [Column("created_at")]
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
}