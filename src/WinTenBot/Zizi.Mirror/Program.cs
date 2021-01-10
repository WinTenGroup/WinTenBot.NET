using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Zizi.Mirror.Utils;

namespace Zizi.Mirror
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            SerilogUtil.SetupLogger();

            Log.Information("Starting Zizi Mirror.");
            try
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e.Demystify(), "Fatal Starting Host.");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseSerilog();
    }
}