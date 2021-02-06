using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Datadog.Logs;
using Serilog.Sinks.SystemConsole.Themes;
using Zizi.Bot.Common;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Extensions
{
    public static class SerilogServiceExtension
    {
        public static void SetupSerilog(this IApplicationBuilder app)
        {
            var templateBase = $"[{{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}";
            var consoleTemplate = $"{{Timestamp:HH:mm:ss.ffffff}} {templateBase}";
            var fileTemplate = $"[{{Timestamp:yyyy-MM-dd HH:mm:ss.fffff zzz}} {templateBase}";
            var logPath = "Storage/Logs/ZiziBot-.log";
            var flushInterval = TimeSpan.FromSeconds(1);
            var rollingInterval = RollingInterval.Day;

            var appConfig = app.ApplicationServices.GetRequiredService<AppConfig>();
            var envConfig = appConfig.EnvironmentConfig;
            var datadogConfig = appConfig.DataDogConfig;

            var datadogKey = datadogConfig.ApiKey;

            var serilogConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .WriteTo.Async(a =>
                    a.File(logPath, rollingInterval: rollingInterval, flushToDiskInterval: flushInterval, shared: true, outputTemplate: consoleTemplate))
                .WriteTo.Async(a =>
                    a.Console(theme: SystemConsoleTheme.Colored, outputTemplate: consoleTemplate));

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

        [Obsolete("Please use the Serilog Log static class directly.")]
        public static string LogInfo(this string message)
        {
            Log.Information(message);
            return message;
        }

        [Obsolete("Please use the Serilog Log static class directly.")]
        public static string LogWarning(this string message)
        {
            Log.Warning(message);
            return message;
        }

        [Obsolete("Please use the Serilog Log static class directly.")]
        public static string LogDebug(this string message)
        {
            Log.Debug(message);
            return message;
        }

        [Obsolete("Please use the Serilog Log static class directly.")]
        public static string LogError(this Exception ex, string message)
        {
            Log.Error(message, ex);
            return message;
        }
    }
}