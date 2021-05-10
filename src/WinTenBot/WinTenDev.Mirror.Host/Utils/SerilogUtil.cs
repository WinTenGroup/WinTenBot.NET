using System;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace WinTenDev.Mirror.Host.Utils
{
    public static class SerilogUtil
    {
        public static void SetupLogger()
        {
            // var consoleStamp = "[{Timestamp:yyyy-MM-dd HH:mm:ss.ffff zzz}";
            // var outputTemplate = $"{consoleStamp} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}";

            var templateBase = $"[{{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}";
            var consoleTemplate = $"{{Timestamp:HH:mm:ss.fffff}} {templateBase}";
            var fileTemplate = $"[{{Timestamp:yyyy-MM-dd HH:mm:ss.ffff zzz}} {templateBase}";
            var logPath = "Storage/Logs/ZiziBot-.log";
            var flushInterval = TimeSpan.FromSeconds(1);
            var rollingInterval = RollingInterval.Day;
            // var datadogKey = BotSettings.DatadogApiKey;
            // var rollingFile = 50 * 1024 * 1024;

            var serilogConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Async(a =>
                    a.File(logPath, rollingInterval: rollingInterval, flushToDiskInterval: flushInterval,
                        shared: true, outputTemplate: consoleTemplate))
                .WriteTo.Async(a =>
                    a.Console(theme: SystemConsoleTheme.Colored, outputTemplate: consoleTemplate));

            // .WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: consoleTemplate);
            // .WriteTo.File(logPath, rollingInterval: rollingInterval, flushToDiskInterval: flushInterval,
            // shared: true, fileSizeLimitBytes: rollingFile, outputTemplate: consoleTemplate);

            // if (BotSettings.IsProduction)
            // {
            //     serilogConfig = serilogConfig
            //         .MinimumLevel.Information()
            //         .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //         .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
            //         .MinimumLevel.Override("Hangfire", LogEventLevel.Information);
            // }
            // else
            // {
            //     serilogConfig = serilogConfig
            //         .MinimumLevel.Debug()
            //         .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
            //         .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Debug)
            //         .MinimumLevel.Override("Hangfire", LogEventLevel.Information);
            // }

            // if (datadogKey != "YOUR_API_KEY" || datadogKey.IsNotNullOrEmpty())
            // {
            //     var dataDogHost = "intake.logs.datadoghq.com";
            //     var config = new DatadogConfiguration(url: dataDogHost, port: 10516, useSSL: true, useTCP: true);
            //     serilogConfig.WriteTo.DatadogLogs(
            //         apiKey: datadogKey,
            //         service: "TelegramBot",
            //         source: BotSettings.DatadogSource,
            //         host: BotSettings.DatadogHost,
            //         tags: BotSettings.DatadogTags.ToArray(),
            //         configuration: config);
            // }

            Log.Logger = serilogConfig.CreateLogger();
        }
    }
}