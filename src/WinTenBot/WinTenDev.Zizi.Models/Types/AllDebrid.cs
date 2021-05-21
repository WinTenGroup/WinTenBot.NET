using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
    public class AllDebrid
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public DebridData DebridData { get; set; }
        
        [JsonProperty("error")]
        public  DebridError DebridError { get; set; }
    }

    public class DebridData
    {
        [JsonProperty("link")]
        public Uri Link { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("streaming")]
        public List<object> Streaming { get; set; }

        [JsonProperty("paws")]
        public bool Paws { get; set; }

        [JsonProperty("filesize")]
        public double Filesize { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("hostDomain")]
        public string HostDomain { get; set; }
    }

    public class DebridError
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}