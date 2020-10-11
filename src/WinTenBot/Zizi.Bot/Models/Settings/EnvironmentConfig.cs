using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Zizi.Bot.Models.Settings
{
    public class EnvironmentConfig
    {
        public bool IsProduction { get; set; }
        public bool IsStaging { get; set; }
        public bool IsDevelopment { get; set; }
        public IWebHostEnvironment HostEnvironment { get; set; }
        public bool IsEnvironment(string envName)
        {
            return HostEnvironment.IsEnvironment(envName);
        }
    }
}
