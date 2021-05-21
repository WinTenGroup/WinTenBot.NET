using System;
using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
    public class CatMeow
    {
        [JsonProperty("file")]
        public Uri File { get; set; }
    }
}