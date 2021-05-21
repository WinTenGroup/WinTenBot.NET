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
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Models.Enums;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Host.Extensions
{
    public static class HangfireServiceExtension
    {
        public static IServiceCollection AddHangfireServerAndConfig(this IServiceCollection services)
        {
            Log.Debug("Adding Hangfire Service");

            var scope = services.BuildServiceProvider();

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
            var serviceProvider = app.GetServiceProvider();
            // var appConfig = app.ApplicationServices.GetRequiredService<AppConfig>();
            // var hangfireConfig = appConfig.HangfireConfig;
            var hangfireConfig = serviceProvider.GetRequiredService<IOptionsSnapshot<HangfireConfig>>().Value;

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
                Queues = hangfireConfig.Queues
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

            var appService = app.GetServiceProvider();

            var rssFeedService = appService.GetRequiredService<RssFeedService>();
            var chatService = appService.GetRequiredService<ChatService>();

            AsyncContext.Run(async () => await rssFeedService.RegisterScheduler());

            AsyncContext.Run(async () => await chatService.RegisterChatHealth());

            // HangfireUtil.RegisterJob<ChatService>("admin-checker", (ChatService service) => service.ChatCleanUp(), Cron.Daily);

            HangfireUtil.RegisterJob<StorageService>("log-cleaner", (StorageService service) => service.ClearLog(), Cron.Daily);

            HangfireUtil.RegisterJob("monkeys-remover", () => MonkeyCacheUtil.DeleteExpired(), Cron.Hourly);

            return app;
        }
    }
}