using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Extensions
{
    public static class MapConfigExtension
    {
        public static IServiceCollection AddMappingConfiguration(this IServiceCollection services)
        {
            Log.Information("Mapping configuration..");
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            var appSettings = config.Get<AppConfig>();
            appSettings.EnvironmentConfig = new EnvironmentConfig()
            {
                HostEnvironment = env,
                IsDevelopment = env.IsDevelopment(),
                IsStaging = env.IsProduction(),
                IsProduction = env.IsProduction()
            };

            services.AddSingleton(appSettings);

            return services;
        }
    }
}
