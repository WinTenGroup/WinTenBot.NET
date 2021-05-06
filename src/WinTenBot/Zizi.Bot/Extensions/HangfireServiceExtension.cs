using System;
using Hangfire;
using Hangfire.Dashboard.Dark;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using Serilog;
using WinTenDev.Models.Enums;
using Zizi.Bot.IO;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Services.HangfireJobs;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Extensions
{
    public static class HangfireServiceExtension
    {
        public static IServiceCollection AddHangfireServerAndConfig(this IServiceCollection services)
        {
            Log.Debug("Adding Hangfire Service");

            var scope = services.BuildServiceProvider();
            var appConfig = scope.GetRequiredService<AppConfig>();
            var hangfireConfig = scope.GetRequiredService<IOptionsSnapshot<HangfireConfig>>().Value;
            var connStrings = scope.GetRequiredService<IOptionsSnapshot<ConnectionStrings>>().Value;

            services.AddHangfireServer()
                .AddHangfire(config =>
                {
                    switch (hangfireConfig.DataStore)
                    {
                        case HangfireDataStore.MySql:
                            config.UseStorage(HangfireUtil.GetMysqlStorage(connStrings.MySql));
                            break;
                        case HangfireDataStore.Sqlite:
                            config.UseStorage(HangfireUtil.GetSqliteStorage(hangfireConfig.Sqlite));
                            break;
                        case HangfireDataStore.Litedb:
                            config.UseStorage(HangfireUtil.GetLiteDbStorage(hangfireConfig.LiteDb));
                            break;
                        case HangfireDataStore.Redis:
                            config.UseStorage(HangfireUtil.GetRedisStorage(hangfireConfig.Redis));
                            break;
                        default:
                            Log.Warning("Unknown Hangfire DataStore");
                            break;
                    }

                    config.UseDarkDashboard()
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseHeartbeatPage(TimeSpan.FromSeconds(15))
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseColouredConsoleLogProvider()
                        .UseSerilogLogProvider();
                });

            Log.Debug("Hangfire Service added..");

            return services;
        }

        public static IApplicationBuilder UseHangfireDashboardAndServer(this IApplicationBuilder app)
        {
            var appConfig = app.ApplicationServices.GetRequiredService<AppConfig>();
            var hangfireConfig = appConfig.HangfireConfig;

            var baseUrl = hangfireConfig.BaseUrl;
            var username = hangfireConfig.Username;
            var password = hangfireConfig.Password;

            Log.Information("Hangfire Url: {HangfireBaseUrl}", baseUrl);
            Log.Information("Hangfire Auth: {HangfireUsername} | {HangfirePassword}", username, password);

            var dashboardOptions = new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangfireCustomBasicAuthenticationFilter
                    {
                        User = username, Pass = password
                    }
                }
            };

            app.UseHangfireDashboard(baseUrl, dashboardOptions);

            var serverOptions = new BackgroundJobServerOptions
            {
                WorkerCount = Environment.ProcessorCount * hangfireConfig.WorkerMultiplier,
                Queues = new[]
                {
                    "default", "rss-feed"
                }
            };

            app.UseHangfireServer(serverOptions, new[]
            {
                new ProcessMonitor(TimeSpan.FromSeconds(3))
            });

            app.RegisterHangfireJobs();

            Log.Information("Hangfire is Running..");
            return app;
        }

        public static IApplicationBuilder RegisterHangfireJobs(this IApplicationBuilder app)
        {
            HangfireUtil.DeleteAllJobs();

            var appScope = app.ApplicationServices.CreateScope();
            var appService = appScope.ServiceProvider;

            var rssFeedService = appService.GetRequiredService<RssFeedService>();

            AsyncContext.Run(async () => await rssFeedService.RegisterScheduler());

            HangfireUtil.RegisterJob<ChatService>("admin-checker", service => service.CheckBotAdminOnGroup(), Cron.Daily);

            HangfireUtil.RegisterJob("log-cleaner", () => Storage.ClearLog(), Cron.Daily);

            HangfireUtil.RegisterJob("monkeys-remover", () => MonkeyCacheUtil.DeleteExpired(), Cron.Hourly);

            return app;
        }
    }
}