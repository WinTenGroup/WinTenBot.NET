using System.Collections.Generic;

namespace WinTenDev.Zizi.Models.Configs
{
    public class DataDogConfig
    {
        public string ApiKey { get; set; }
        public string Host { get; set; }
        public string Source { get; set; }
        public List<string> Tags { get; set; }
    }
}
