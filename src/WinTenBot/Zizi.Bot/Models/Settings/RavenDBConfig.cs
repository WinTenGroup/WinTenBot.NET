using System.Collections.Generic;

namespace Zizi.Bot.Models.Settings
{
    public class RavenDBConfig
    {
        public string CertPath { get; set; }
        public string DbName { get; set; }
        public List<string> Nodes { get; set; }
    }
}
