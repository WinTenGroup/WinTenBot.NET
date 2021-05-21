using System;
using Newtonsoft.Json;

namespace WinTenDev.WebHook.Host.Models.Github
{
    public partial class Organization
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("repos_url")]
        public Uri ReposUrl { get; set; }

        [JsonProperty("events_url")]
        public Uri EventsUrl { get; set; }

        [JsonProperty("hooks_url")]
        public Uri HooksUrl { get; set; }

        [JsonProperty("issues_url")]
        public Uri IssuesUrl { get; set; }

        [JsonProperty("members_url")]
        public string MembersUrl { get; set; }

        [JsonProperty("public_members_url")]
        public string PublicMembersUrl { get; set; }

        [JsonProperty("avatar_url")]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}