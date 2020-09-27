using System;
using Newtonsoft.Json;

namespace Zizi.Bot.Model
{
    public class CatMeow
    {
        [JsonProperty("file")]
        public Uri File { get; set; }
    }
}