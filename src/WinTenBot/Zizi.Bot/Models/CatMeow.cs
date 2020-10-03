using System;
using Newtonsoft.Json;

namespace Zizi.Bot.Models
{
    public class CatMeow
    {
        [JsonProperty("file")]
        public Uri File { get; set; }
    }
}