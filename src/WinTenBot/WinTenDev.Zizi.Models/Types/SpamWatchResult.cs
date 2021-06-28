using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
    public partial class SpamWatchResult
    {
        [JsonProperty("admin")]
        public long Admin { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        public bool IsBan { get; set; }
    }
}