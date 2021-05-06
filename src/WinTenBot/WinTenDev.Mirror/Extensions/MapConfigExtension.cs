using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zizi.Core.Models.Settings;

namespace WinTenDev.Mirror.Extensions
{
    public static class MapConfigExtension
    {
        public static IServiceCollection AddMapConfiguration(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            if (configuration == null) return services;

            var appSettings = configuration.Get<AppConfig>();

            // appSettings.EnvironmentConfig = new EnvironmentConfig()
            // {
            //     HostEnvironment = environment,
            //     IsDevelopment = environment.IsDevelopment(),
            //     IsStaging = environment.IsProduction(),
            //     IsProduction = environment.IsProduction()
            // };

            services.AddSingleton(configuration);
            services.AddSingleton(appSettings);

            return services;
        }
    }
}