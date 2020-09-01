using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace WinTenBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Log.Information("Starting WebAPI..");
                // BuildWebHost(args).Run();
                await CreateWebHostBuilder(args)
                    .Build()
                    .RunAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Demystify(), "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostBuilder, configBuilder) => configBuilder
                    .AddJsonFile("appsettings.json", true, true)
                    // .AddJsonFile($"appsettings.{hostBuilder.HostingEnvironment.EnvironmentName}.json", true, true)
                    // .AddJsonFile("Storage/Config/security-base.json", true, true)
                    .AddJsonEnvVar("QUICKSTART_SETTINGS", true)
                ).UseStartup<Startup>()
                .UseSerilog();
        }
    }
}