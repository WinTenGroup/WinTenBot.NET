using Newtonsoft.Json;

namespace Zizi.Bot.Models
{
    public class ButtonMarkup
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
