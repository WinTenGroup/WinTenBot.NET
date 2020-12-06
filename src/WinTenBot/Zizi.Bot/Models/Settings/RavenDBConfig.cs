using System.Collections.Generic;

namespace Zizi.Bot.Models.Settings
{
    public class RavenDBConfig
    {
        public Embedded Embedded { get; set; }
        public string CertPath { get; set; }
        public string DbName { get; set; }
        public List<string> Nodes { get; set; }
    }

    public class Embedded
    {
        public string ServerUrl { get; set; }
        public string FrameworkVersion { get; set; }
    }
}
