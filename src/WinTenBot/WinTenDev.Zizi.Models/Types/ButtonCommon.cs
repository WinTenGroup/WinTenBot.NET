using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
    public class ButtonMarkup
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
