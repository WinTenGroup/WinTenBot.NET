using System;
using System.Diagnostics;
using System.IO;
using Dapper;
using Dapper.FluentMap;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonkeyCache;
using MonkeyCache.FileStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Telegram.Bot;
using WinTenDev.Zizi.Host.Handlers.Callbacks;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Services.Interfaces;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.IO;

namespace WinTenDev.Zizi.Host.Extensions
{
    public static class CommonServiceExtension
    {
        public static IServiceCollection AddFeatureServices(this IServiceCollection services)
        {
            services
                .AddScoped<AfkService>()
                .AddScoped<AllDebridService>()
                .AddScoped<AntiSpamService>()
                .AddScoped<BlockListService>()
                .AddScoped<CekResiService>()
                .AddScoped<ChatService>()
                .AddScoped<GlobalBanService>()
                .AddScoped<IWeatherService, WeatherService>()
                .AddScoped<LearningService>()
                .AddScoped<LmaoCovidService>()
                .AddScoped<MataService>()
                .AddScoped<MediaFilterService>()
                .AddScoped<NotesService>()
                .AddScoped<QueryService>()
                .AddScoped<RssFeedService>()
                .AddScoped<RssService>()
                .AddScoped<SettingsService>()
                .AddScoped<TagsService>()
                .AddScoped<TelegramService>()
                .AddScoped<WeatherService>()
                .AddScoped<WordFilterService>()
                .AddScoped<StorageService>()
                .AddScoped(service =>
                {
                    var configuration = service.GetRequiredService<IConfiguration>();

                    var token = configuration.GetSection("ZiziBot:ApiToken").Value;
                    var client = new TelegramBotClient(token);

                    return client;
                })
                .AddScoped<DeepAiService>();

            return services;
        }

        public static IServiceCollection AddCallbackQueryHandlers(this IServiceCollection services)
        {
            return services.AddScoped<SettingsCallback>()
                .AddScoped<ActionCallback>()
                .AddScoped<HelpCallback>()
                .AddScoped<PingCallback>()
                .AddScoped<VerifyCallback>();
        }

        public static IApplicationBuilder ConfigureNewtonsoftJson(this IApplicationBuilder app)
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                ContractResolver = contractResolver,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            return app;
        }

        public static IApplicationBuilder ConfigureDapper(this IApplicationBuilder app)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            FluentMapper.Initialize(configuration => { configuration.AddMap(new ChatSettingMap()); });

            return app;
        }

        [Obsolete("MonkeyCache no longer used for Caching.")]
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

            Log.Information("MonkeyCache is ready. TimeUtil: {0}", sw.Elapsed);
            sw.Stop();

            return app;
        }
    }
}