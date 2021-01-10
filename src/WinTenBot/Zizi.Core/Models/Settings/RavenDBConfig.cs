using System.Collections.Generic;

namespace Zizi.Core.Models.Settings
{
    public class RavenDBConfig
    {
        public string CertPath { get; set; }
        public string DbName { get; set; }
        public List<string> Nodes { get; set; }
    }
}
