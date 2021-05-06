using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

            services.AddSingleton(appSettings.EnginesConfig);
            services.AddSingleton(appSettings.CommonConfig);
            services.AddSingleton(appSettings.DatabaseConfig);
            services.AddSingleton(appSettings.HangfireConfig);
            services.AddSingleton(appSettings.ConnectionStrings);
            services.AddSingleton(appSettings.DataDogConfig);

            return services;
        }

        public static IServiceCollection MappingAppSettings(this IServiceCollection services)
        {
            Log.Information("Mapping configuration..");
            var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            services.Configure<AllDebridConfig>(config.GetSection(nameof(AllDebridConfig)));
            services.Configure<CommonConfig>(config.GetSection(nameof(CommonConfig)));
            services.Configure<ConnectionStrings>(config.GetSection(nameof(ConnectionStrings)));
            services.Configure<DatabaseConfig>(config.GetSection(nameof(DatabaseConfig)));
            services.Configure<DataDogConfig>(config.GetSection(nameof(DataDogConfig)));
            services.Configure<EnginesConfig>(config.GetSection(nameof(EnginesConfig)));
            services.Configure<GoogleCloudConfig>(config.GetSection(nameof(GoogleCloudConfig)));
            services.Configure<HangfireConfig>(config.GetSection(nameof(HangfireConfig)));
            services.Configure<UptoboxConfig>(config.GetSection(nameof(UptoboxConfig)));

            var engines = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<EnginesConfig>>();

            return services;
        }
    }
}