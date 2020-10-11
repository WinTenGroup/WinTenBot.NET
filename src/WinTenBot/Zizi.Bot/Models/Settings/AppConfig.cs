using System.Collections.Generic;

namespace Zizi.Bot.Models.Settings
{
    public class AppConfig
    {
        public EnginesConfig EnginesConfig { get; set; }
        public List<string> Sudoers { get; set; }
        public List<string> RestrictArea { get; set; }
        public CommonConfig CommonConfig { get; set; }
        public DataDogConfig DataDogConfig { get; set; }
        public EnvironmentConfig EnvironmentConfig { get; set; }
        public GoogleCloudConfig GoogleCloudConfig { get; set; }
        public HangfireConfig HangfireConfig { get; set; }
        public OcrSpaceConfig OcrSpaceConfig { get; set; }
        public RavenDBConfig RavenDBConfig { get; set; }
        public UptoboxConfig UptoboxConfig { get; set; }
    }
}
