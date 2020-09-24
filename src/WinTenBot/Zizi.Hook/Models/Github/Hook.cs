using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Zizi.Hook.Models.Github
{
    public class Hook
    {
        [JsonProperty("type")]
        public string Type
        {
            get; set;
        }

        [JsonProperty("id")]
        public long Id
        {
            get; set;
        }

        [JsonProperty("name")]
        public string Name
        {
            get; set;
        }

        [JsonProperty("active")]
        public bool Active
        {
            get; set;
        }

        [JsonProperty("events")]
        public List<string> Events
        {
            get; set;
        }

        [JsonProperty("config")]
        public Config Config
        {
            get; set;
        }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt
        {
            get; set;
        }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt
        {
            get; set;
        }

        [JsonProperty("url")]
        public Uri Url
        {
            get; set;
        }

        [JsonProperty("test_url")]
        public Uri TestUrl
        {
            get; set;
        }

        [JsonProperty("ping_url")]
        public Uri PingUrl
        {
            get; set;
        }

        [JsonProperty("last_response")]
        public LastResponse LastResponse
        {
            get; set;
        }
    }
}