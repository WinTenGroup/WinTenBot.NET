using System;
using Newtonsoft.Json;

namespace Zizi.Bot.Models.Uptobox
{
    public class UptoboxUser
    {
        [JsonProperty("statusCode")]
        public long StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public UserData UserData { get; set; }
    }

    public class UserData
    {
        [JsonProperty("premium")]
        public bool Premium { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("point")]
        public string Point { get; set; }

        [JsonProperty("premium_expire")]
        public DateTime PremiumExpire { get; set; }

        [JsonProperty("securityLock")]
        public bool SecurityLock { get; set; }

        [JsonProperty("directDownload")]
        public bool DirectDownload { get; set; }

        [JsonProperty("sslDownload")]
        public bool SslDownload { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }


}
