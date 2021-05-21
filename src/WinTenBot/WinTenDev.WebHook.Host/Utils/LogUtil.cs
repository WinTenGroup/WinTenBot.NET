using System;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace WinTenDev.WebHook.Host.Utils
{
    public static class LogUtil
    {
        public static void Setup()
        {
            const string outputTemplate = "[{Timestamp:HH:mm:ss.ffff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            var logPath = "Storage/Logs/ZiziHook-.log";
            var flushInterval = TimeSpan.FromSeconds(1);
            var rollingInterval = RollingInterval.Day;

            var config = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Debug)
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: outputTemplate)
                .WriteTo.File(logPath, rollingInterval: rollingInterval, flushToDiskInterval: flushInterval,
                    retainedFileCountLimit: 7, outputTemplate: outputTemplate);

            Log.Logger = config.CreateLogger();
        }
    }
}