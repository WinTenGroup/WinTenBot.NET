using Newtonsoft.Json;

namespace Zizi.Bot.Models.Uptobox
{
    public class UptoboxLink
    {
        [JsonProperty("statusCode")]
        public long StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public LinkData LinkData { get; set; }
    }

    public class LinkData
    {
        [JsonProperty("dlLink")]
        public string DlLink { get; set; }
    }
}
