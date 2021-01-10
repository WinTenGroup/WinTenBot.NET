using System;
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using Hangfire.LiteDB;
using HangfireBasicAuthenticationFilter;
using LiteDB.Async;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Zizi.Core.Models.Settings;

namespace Zizi.Mirror.Extensions
{
    public static class HangfireServiceExtension
    {
        public static IServiceCollection AddHangfireServerAndConfig(this IServiceCollection services)
        {
            Log.Debug("Adding Hangfire Service");

            var scope = services.BuildServiceProvider();
            var appConfig = scope.GetService<AppConfig>();

            if (appConfig == null) return services;

            var connStr = appConfig.HangfireConfig.LiteDb;

            var db = new LiteDatabaseAsync(connStr);
            db.Dispose();

            services.AddHangfireServer()
                .AddHangfire(config =>
                {
                    config
                        .UseSerilogLogProvider()
                        .UseStorage(GetLiteDbStorage(connStr))
                        // .UseStorage(HangfireJobs.GetSqliteStorage())
                        // .UseStorage(HangfireJobs.GetLiteDbStorage())
                        // .UseStorage(HangfireJobs.GetRedisStorage())
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        // .UseDarkDashboard()
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
            var service = app.ApplicationServices;
            var appConfig = service.GetRequiredService<AppConfig>();

            var hangfireBaseUrl = appConfig.HangfireConfig.BaseUrl;
            var hangfireUsername = appConfig.HangfireConfig.Username;
            var hangfirePassword = appConfig.HangfireConfig.Password;

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

            // BotScheduler.StartScheduler();


            return app;
        }

        public static LiteDbStorage GetLiteDbStorage(string connectionString)
        {
            Log.Information("HangfireLiteDb: {0}", connectionString);

            var options = new LiteDbStorageOptions()
            {
                QueuePollInterval = TimeSpan.FromSeconds(10)
            };

            var storage = new LiteDbStorage(connectionString, options);
            return storage;
        }
    }
}