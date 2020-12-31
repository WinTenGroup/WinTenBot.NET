using System;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Datadog.Logs;
using Serilog.Sinks.SystemConsole.Themes;
using Zizi.Bot.Models;

namespace Zizi.Bot.Common
{
    public static class Logger
    {
        public static void SetupLogger()
        {
            var templateBase = $"[{{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}";
            var consoleTemplate = $"{{Timestamp:HH:mm:ss.fffff}} {templateBase}";
            var fileTemplate = $"[{{Timestamp:yyyy-MM-dd HH:mm:ss.fffff zzz}} {templateBase}";
            var logPath = "Storage/Logs/ZiziBot-.log";
            var flushInterval = TimeSpan.FromSeconds(1);
            var rollingInterval = RollingInterval.Day;
            var datadogKey = BotSettings.DatadogApiKey;

            var serilogConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .WriteTo.Async(a =>
                    a.File(logPath, rollingInterval: rollingInterval, flushToDiskInterval: flushInterval, shared: true, outputTemplate: consoleTemplate))
                .WriteTo.Async(a =>
                    a.Console(theme: SystemConsoleTheme.Colored, outputTemplate: consoleTemplate));

            if (BotSettings.IsProduction)
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
                    source: BotSettings.DatadogSource,
                    host: BotSettings.DatadogHost,
                    tags: BotSettings.DatadogTags.ToArray(),
                    configuration: config);
            }

            Log.Logger = serilogConfig.CreateLogger();
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