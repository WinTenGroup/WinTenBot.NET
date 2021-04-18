using System;
using System.Diagnostics;
using System.IO;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonkeyCache;
using MonkeyCache.FileStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Telegram.Bot;
using Zizi.Bot.Handlers.Callbacks;
using Zizi.Bot.Interfaces;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Services.HangfireJobs;
using Zizi.Bot.Tools;
using Zizi.Core.Utils;

namespace Zizi.Bot.Extensions
{
    public static class CommonServiceExtension
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            return services
                .AddScoped<QueryService>()
                .AddScoped<AfkService>()
                .AddScoped<GlobalBanService>()
                .AddScoped<RssService>()
                .AddScoped<SettingsService>()
                .AddScoped<TagsService>()
                .AddScoped<NotesService>()
                .AddScoped<MediaFilterService>()
                .AddScoped<LearningService>()
                .AddScoped<WordFilterService>();
        }

        public static IServiceCollection AddFeatureServices(this IServiceCollection services)
        {
            return services
                .AddScoped<AntiSpamService>()
                .AddScoped<IWeatherService, WeatherService>()
                .AddScoped<TelegramService>()
                .AddScoped<ChatService>()
                .AddScoped<RssFeedService>()
                .AddScoped<CekResiService>()
                .AddScoped(service =>
                {
                    var configuration = service.GetRequiredService<IConfiguration>();

                    var token = configuration.GetSection("ZiziBot:ApiToken").Value;
                    var client = new TelegramBotClient(token);

                    return client;
                })
                .AddScoped<DeepAiService>();
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
            var contractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
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

            return app;
        }

        [Obsolete("MonkeyCache no longer used")]
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