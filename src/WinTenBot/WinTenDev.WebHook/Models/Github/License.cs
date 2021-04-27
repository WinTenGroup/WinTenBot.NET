#region

using System;
using Newtonsoft.Json;

#endregion

namespace WinTenDev.WebHook.Models.Github
{
    public class License
    {
        [JsonProperty("key")]
        public string Key
        {
            get; set;
        }

        [JsonProperty("name")]
        public string Name
        {
            get; set;
        }

        [JsonProperty("spdx_id")]
        public string SpdxId
        {
            get; set;
        }

        [JsonProperty("url")]
        public Uri Url
        {
            get; set;
        }

        [JsonProperty("node_id")]
        public string NodeId
        {
            get; set;
        }
    }
}