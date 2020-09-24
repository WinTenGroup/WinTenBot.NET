#region

using Newtonsoft.Json;

#endregion

namespace Zizi.Hook.Models.Github
{
    public class LastResponse
    {
        [JsonProperty("code")]
        public object Code
        {
            get; set;
        }

        [JsonProperty("status")]
        public string Status
        {
            get; set;
        }

        [JsonProperty("message")]
        public object Message
        {
            get; set;
        }
    }
}