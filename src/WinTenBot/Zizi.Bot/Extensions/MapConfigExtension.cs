using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Extensions
{
    public static class MapConfigExtension
    {
        public static IServiceCollection AddMapConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (configuration == null) return services;

            var appSettings = configuration.Get<AppConfig>();
            appSettings.EnvironmentConfig = new EnvironmentConfig()
            {
                HostEnvironment = environment,
                IsDevelopment = environment.IsDevelopment(),
                IsStaging = environment.IsProduction(),
                IsProduction = environment.IsProduction()
            };

            services.AddSingleton(configuration);
            services.AddSingleton(appSettings);

            return services;
        }
    }
}
