using System;
using Hangfire;
using Hangfire.Dashboard.Dark;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Zizi.Bot.Models;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Scheduler;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Extensions
{
    public static class HangfireServiceExtension
    {
        public static IServiceCollection AddHangfireServerAndConfig(this IServiceCollection services)
        {
            Log.Debug("Adding Hangfire Service");

            var scope = services.BuildServiceProvider();
            var appConfig = scope.GetService<AppConfig>();

            if (appConfig == null) return services;

            var connStr = appConfig.HangfireConfig.Mysql;

            services.AddHangfireServer()
                .AddHangfire(config =>
                {
                    config
                        .UseSerilogLogProvider()
                        .UseStorage(HangfireJobs.GetMysqlStorage(connStr))
                        // .UseStorage(HangfireJobs.GetSqliteStorage())
                        // .UseStorage(HangfireJobs.GetLiteDbStorage())
                        // .UseStorage(HangfireJobs.GetRedisStorage())
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseDarkDashboard()
                        .UseHeartbeatPage(TimeSpan.FromSeconds(5))
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseColouredConsoleLogProvider();
                });

            Log.Debug("Hangfire Service added.");

            return services;
        }

        public static IApplicationBuilder UseHangfireDashboardAndServer(this IApplicationBuilder app)
        {
            var hangfireBaseUrl = BotSettings.HangfireBaseUrl;
            var hangfireUsername = BotSettings.HangfireUsername;
            var hangfirePassword = BotSettings.HangfirePassword;

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
                WorkerCount = Environment.ProcessorCount * 2
            };

            app.UseHangfireServer(serverOptions, new[]
            {
                new ProcessMonitor(TimeSpan.FromSeconds(1))
            });

            BotScheduler.StartScheduler();


            return app;
        }
    }
}