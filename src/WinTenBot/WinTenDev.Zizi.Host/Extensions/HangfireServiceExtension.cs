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
using Nito.AsyncEx.Synchronous;
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

        public static IApplicationBuilder ResetHangfireStorageIfRequired(this IApplicationBuilder app)
        {
            var serviceProvider = app.GetServiceProvider();

            var storageService = serviceProvider.GetRequiredService<StorageService>();
            var parseError = ErrorUtil.ParseErrorTextAsync().Result;
            var lastFtlError = parseError.FullText;

            if (lastFtlError.Contains("hangfire", StringComparison.CurrentCultureIgnoreCase))
            {
                if (lastFtlError.Contains("Storage", StringComparison.CurrentCultureIgnoreCase))
                {
                    Log.Warning("Last error about Hangfire, seem need to Reset Storage");
                    storageService.ResetHangfire(ResetTableMode.Truncate).WaitAndUnwrapException();

                    "".SaveErrorToText().WaitAndUnwrapException();
                }
            }

            return app;
        }

        public static IApplicationBuilder UseHangfireDashboardAndServer(this IApplicationBuilder app)
        {
            var serviceProvider = app.GetServiceProvider();
            var hangfireConfig = serviceProvider.GetRequiredService<IOptionsSnapshot<HangfireConfig>>().Value;

            var baseUrl = hangfireConfig.BaseUrl;
            var username = hangfireConfig.Username;
            var password = hangfireConfig.Password;

            Log.Information("Hangfire Url: {HangfireBaseUrl}", baseUrl);
            Log.Information("Hangfire Auth: {HangfireUsername} | {HangfirePassword}", username, password);

            app.ResetHangfireStorageIfRequired();

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

            rssFeedService.RegisterScheduler().Ignore();
            chatService.RegisterChatHealth().Ignore();

            HangfireUtil.RegisterJob<StorageService>("log-cleaner", (StorageService service) => service.ClearLog(), Cron.Daily);
            HangfireUtil.RegisterJob("monkeys-remover", () => MonkeyCacheUtil.DeleteExpired(), Cron.Hourly);

            return app;
        }
    }
}