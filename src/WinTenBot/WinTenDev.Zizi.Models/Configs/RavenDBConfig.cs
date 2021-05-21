using System.Collections.Generic;

namespace WinTenDev.Zizi.Models.Configs
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
