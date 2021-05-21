using Microsoft.Extensions.Hosting;

namespace WinTenDev.Zizi.Models.Configs
{
    public class EnvironmentConfig
    {
        public bool IsProduction { get; set; }
        public bool IsStaging { get; set; }
        public bool IsDevelopment { get; set; }
        public IHostEnvironment HostEnvironment { get; set; }

        public bool IsEnvironment(string envName)
        {
            return HostEnvironment.IsEnvironment(envName);
        }
    }
}