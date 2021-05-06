using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Zizi.Bot.Extensions;

namespace Zizi.Bot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await CreateHostBuilder(args)
                    .Build()
                    .RunAsync();
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

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", true, true);
                })
                .ConfigureServices((context, services) =>
                {
                    webBuilder.UseSerilog();
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddSerilog();
                });

            // if (Directory.Exists("wwwroot")) hostBuilder.UseContentRoot("wwwroot");

            return hostBuilder;
        }

        [Obsolete("WebHostBuilder will replaced with CreateHostBuilder")]
        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((webHostBuilder, configBuilder) =>
                {
                    configBuilder
                        .AddJsonFile("appsettings.json", true, true)
                        // .AddJsonFile($"appsettings.{hostBuilder.HostingEnvironment.EnvironmentName}.json", true, true)
                        // .AddJsonFile("Storage/Config/security-base.json", true, true)
                        .AddJsonEnvVar("QUICKSTART_SETTINGS", true);
                }).UseStartup<Startup>()
                .UseSerilog();
        }
    }
}