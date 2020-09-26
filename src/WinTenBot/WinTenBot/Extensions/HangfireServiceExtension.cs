using System;
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WinTenBot.Model;
using WinTenBot.Scheduler;
using WinTenBot.Tools;

namespace WinTenBot.Extensions
{
    public static class HangfireServiceExtension
    {
        public static IServiceCollection AddHangfireServerAndConfig(this IServiceCollection services)
        {
            return services.AddHangfireServer()
                .AddHangfire(config =>
                {
                    config
                        .UseStorage(HangfireJobs.GetMysqlStorage())
                        // config.UseStorage(HangfireJobs.GetSqliteStorage())
                        // config.UseStorage(HangfireJobs.GetLiteDbStorage())
                        // config.UseStorage(HangfireJobs.GetRedisStorage())
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseHeartbeatPage(TimeSpan.FromSeconds(5))
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseSerilogLogProvider()
                        .UseColouredConsoleLogProvider();
                });
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