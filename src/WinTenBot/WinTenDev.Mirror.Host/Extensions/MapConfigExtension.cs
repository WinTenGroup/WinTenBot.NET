using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WinTenDev.Zizi.Models.Configs;

namespace WinTenDev.Mirror.Host.Extensions
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