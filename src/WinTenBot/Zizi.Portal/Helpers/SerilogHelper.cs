using System;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Zizi.Portal.Helpers
{
    public class SerilogHelper
    {
        public static void SetupSerilog()
        {
            const string outputTemplate = "[{Timestamp:HH:mm:ss.ffff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            var logPath = "Storage/Logs/ZiziPortal-.log";
            var flushInterval = TimeSpan.FromSeconds(1);
            var rollingInterval = RollingInterval.Day;

            var serilogConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Debug)
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored, outputTemplate: outputTemplate)
                .WriteTo.File(logPath, rollingInterval: rollingInterval, flushToDiskInterval: flushInterval,
                    retainedFileCountLimit: 7);

            Log.Logger = serilogConfig.CreateLogger();
        }
    }
}