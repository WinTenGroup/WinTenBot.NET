using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Datadog.Logs;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Templates;
using Zizi.Bot.Common;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Extensions
{
    public static class SerilogServiceExtension
    {
        public static void SetupSerilog(this IApplicationBuilder app)
        {
            const string logPath = "Storage/Logs/ZiziBot-.log";
            const RollingInterval rollingInterval = RollingInterval.Day;
            var flushInterval = TimeSpan.FromSeconds(1);

            var templateBase = $"[{{Level:u3}}] {{MemoryUsage}}{{ThreadId}}| {{Message:lj}}{{NewLine}}{{Exception}}";
            var outputTemplate = $"{{Timestamp:HH:mm:ss.fff}} {templateBase}";

            var appConfig = app.ApplicationServices.GetRequiredService<AppConfig>();
            var envConfig = appConfig.EnvironmentConfig;

            var datadogConfig = appConfig.DataDogConfig;
            var datadogKey = datadogConfig.ApiKey;

            var serilogConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                // .Enrich.WithThreadId()
                // .Enrich.WithPrettiedMemoryUsage()
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .MinimumLevel.Override("MySqlConnector", LogEventLevel.Warning)
                // .Filter.ByExcluding("{@m} not like '%pinged server%'")
                .WriteTo.Async(a =>
                    a.File(logPath, rollingInterval: rollingInterval, flushToDiskInterval: flushInterval, shared: true, outputTemplate: outputTemplate))
                .WriteTo.Async(a =>
                    a.Console(theme: SystemConsoleTheme.Colored, outputTemplate: outputTemplate))
                .WriteTo.Async(configuration => configuration.Sentry(options =>
                {
                    options.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                    options.MinimumEventLevel = LogEventLevel.Warning;
                }));

            if (envConfig.IsProduction)
            {
                serilogConfig.MinimumLevel.Information();
            }
            else
            {
                serilogConfig.MinimumLevel.Debug();
            }

            if (datadogKey != "YOUR_API_KEY" || datadogKey.IsNotNullOrEmpty())
            {
                var dataDogHost = "intake.logs.datadoghq.com";
                var config = new DatadogConfiguration(url: dataDogHost, port: 10516, useSSL: true, useTCP: true);

                serilogConfig.WriteTo.DatadogLogs(
                    apiKey: datadogKey,
                    service: "TelegramBot",
                    source: datadogConfig.Source,
                    host: datadogConfig.Host,
                    tags: datadogConfig.Tags.ToArray(),
                    configuration: config);
            }

            Log.Logger = serilogConfig.CreateLogger();

            app.UseSerilogRequestLogging();
        }
    }
}