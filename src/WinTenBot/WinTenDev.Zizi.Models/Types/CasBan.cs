using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
    public class CasBan
    {
        [JsonProperty("ok")] 
        public bool Ok { get; set; }

        [JsonProperty("result")] 
        public Result Result { get; set; }
    }

    public class Result
    {
        [JsonProperty("offenses")] 
        public long Offenses { get; set; }

        [JsonProperty("messages")] 
        public List<string> Messages { get; set; }

        [JsonProperty("time_added")]
        public DateTimeOffset TimeAdded { get; set; }
    }
}