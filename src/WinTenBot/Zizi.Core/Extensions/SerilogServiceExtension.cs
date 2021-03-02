using System;
using System.Diagnostics;
using System.Threading;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Zizi.Core.Utils;

namespace Zizi.Core.Extensions
{
    public static class SerilogServiceExtension
    {
        public static LoggerConfiguration WithThreadId(this LoggerEnrichmentConfiguration enrich)
        {
            return enrich.WithDynamicProperty("ThreadId", () =>
            {
                var threadId = Thread.CurrentThread.ManagedThreadId.ToString();
                return $"ThreadId: {threadId} ";
            });
        }

        public static LoggerConfiguration WithPrettiedMemoryUsage(this LoggerEnrichmentConfiguration configuration)
        {

            return configuration.WithDynamicProperty("MemoryUsage", () =>
            {
                GC.Collect();

                var proc = Process.GetCurrentProcess();
                var mem = proc.PrivateMemorySize64.SizeFormat();
                // var memUsage = GC.GetTotalMemory(true).SizeFormat();

                return $"{mem} ";
            });
        }
    }
}