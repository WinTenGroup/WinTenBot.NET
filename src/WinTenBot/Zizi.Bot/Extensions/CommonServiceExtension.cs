using System.Diagnostics;
using System.IO;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MonkeyCache;
using MonkeyCache.FileStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Zizi.Bot.Interfaces;
using Zizi.Bot.Services;
using Zizi.Bot.Services.HangfireJobs;
using Zizi.Bot.Tools;
using Zizi.Core.Utils;

namespace Zizi.Bot.Extensions
{
    public static class CommonServiceExtension
    {
        public static IServiceCollection AddDataService(this IServiceCollection services)
        {
            return services
                .AddScoped<TagsService>()
                .AddScoped<SettingsService>();
        }

        public static IServiceCollection AddFeatureService(this IServiceCollection services)
        {
            return services
                .AddScoped<IWeatherService, WeatherService>()
                .AddScoped<ChatService>()
                .AddScoped<CekResiService>();
        }

        public static IApplicationBuilder ConfigureNewtonsoftJson(this IApplicationBuilder app)
        {
            var contractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                ContractResolver = contractResolver,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return app;
        }

        public static IApplicationBuilder ConfigureDapper(this IApplicationBuilder app)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            return app;
        }

        public static IApplicationBuilder UseMonkeyCache(this IApplicationBuilder app)
        {
            var sw = Stopwatch.StartNew();
            var appId = "ZiziBot-Cache";
            var cachePath = Path.Combine("Storage", "MonkeyCache").SanitizeSlash().EnsureDirectory(true);

            Log.Information("Initializing MonkeyCache. appId: {0}", appId);
            Log.Debug("Path: {0}", cachePath);

            Barrel.ApplicationId = appId;
            BarrelUtils.SetBaseCachePath(cachePath);

            Log.Debug("Deleting old MonkeyCache");
            MonkeyCacheUtil.DeleteKeys();

            Log.Information("MonkeyCache is ready. Time: {0}", sw.Elapsed);
            sw.Stop();

            return app;
        }
    }
}