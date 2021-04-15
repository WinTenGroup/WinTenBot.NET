using System;
using Hangfire;
using Hangfire.Dashboard.Dark;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using Serilog;
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

            var connStr = appConfig.ConnectionStrings.MySql;

            services.AddHangfireServer()
                .AddHangfire(config =>
                {
                    config
                        .UseStorage(HangfireUtil.GetMysqlStorage(connStr))
                        // .UseStorage(HangfireJobs.GetSqliteStorage())
                        // .UseStorage(HangfireJobs.GetLiteDbStorage())
                        // .UseStorage(HangfireJobs.GetRedisStorage())
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseDarkDashboard()
                        .UseHeartbeatPage(TimeSpan.FromSeconds(5))
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseColouredConsoleLogProvider()
                        .UseSerilogLogProvider();
                });

            Log.Debug("Hangfire Service added.");

            return services;
        }

        public static IApplicationBuilder UseHangfireDashboardAndServer(this IApplicationBuilder app)
        {
            var appConfig = app.ApplicationServices.GetRequiredService<AppConfig>();
            var hangfireConfig = appConfig.HangfireConfig;

            var hangfireBaseUrl = hangfireConfig.BaseUrl;
            var hangfireUsername = hangfireConfig.Username;
            var hangfirePassword = hangfireConfig.Password;

            Log.Information("Hangfire Url: {0}", hangfireBaseUrl);
            Log.Information("Hangfire Auth: {0} | {1}", hangfireUsername, hangfirePassword);

            var dashboardOptions = new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangfireCustomBasicAuthenticationFilter {User = hangfireUsername, Pass = hangfirePassword}
                }
            };

            app.UseHangfireDashboard(hangfireBaseUrl, dashboardOptions);

            var serverOptions = new BackgroundJobServerOptions
            {
                WorkerCount = Environment.ProcessorCount * hangfireConfig.WorkerMultiplier,
                Queues = new []{"default","rss-feed"}
            };

            app.UseHangfireServer(serverOptions, new[]
            {
                new ProcessMonitor(TimeSpan.FromSeconds(1))
            });

            app.RegisterHangfireOnStartup();

            Log.Information("Hangfire is Running..");
            return app;
        }

        public static IApplicationBuilder RegisterHangfireOnStartup(this IApplicationBuilder app)
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