using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinTenDev.Mirror.Host.Utils;
using WinTenDev.Zizi.DbContexts;

namespace WinTenDev.Mirror.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            SerilogUtil.SetupLogger();

            Log.Information("Starting Zizi Mirror.");
            try
            {
                var host = CreateHostBuilder(args).Build();

                CreateDbIfNotExists(host);

                await host.RunAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e.Demystify(), "Fatal Starting Host.");
                Log.CloseAndFlush();
            }
        }

        private static void CreateDbIfNotExists(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AuthorizationContext>();
                context.Database.EnsureCreated();
                // DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                // var logger = services.GetRequiredService<ILogger<Program>>();
                Log.Error(ex, "An error occurred creating the DB.");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}