using Newtonsoft.Json;

namespace WinTenDev.WebHook.Host.Models.Github
{
    public partial class RootAction
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("repository")]
        public Repository Repository { get; set; }

        [JsonProperty("organization")]
        public Organization Organization { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }
    }
}