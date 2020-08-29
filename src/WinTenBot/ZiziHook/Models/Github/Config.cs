using Newtonsoft.Json;
using System;

namespace ZiziHook.Models.Github
{
    public class Config
    {
        [JsonProperty("content_type")]
        public string ContentType
        {
            get; set;
        }

        [JsonProperty("insecure_ssl")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long InsecureSsl
        {
            get; set;
        }

        [JsonProperty("url")]
        public Uri Url
        {
            get; set;
        }
    }
}