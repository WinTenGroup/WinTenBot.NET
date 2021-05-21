using System.Collections.Generic;

namespace WinTenDev.Zizi.Models.Configs
{
    public class AppConfig
    {
        public EnginesConfig EnginesConfig { get; set; }
        public List<string> Sudoers { get; set; }
        public List<string> RestrictArea { get; set; }

        public CommonConfig CommonConfig { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public EnvironmentConfig EnvironmentConfig { get; set; }
        
        public RavenDBConfig RavenDBConfig { get; set; }
        public DatabaseConfig DatabaseConfig { get; set; }
        public HangfireConfig HangfireConfig { get; set; }
        public DataDogConfig DataDogConfig { get; set; }
        
        public AllDebridConfig AllDebridConfig { get; set; }
        public PigooraConfig PigooraConfig { get; set; }
        public GoogleCloudConfig GoogleCloudConfig { get; set; }
        public OcrSpaceConfig OcrSpaceConfig { get; set; }
        public UptoboxConfig UptoboxConfig { get; set; }
    }
}