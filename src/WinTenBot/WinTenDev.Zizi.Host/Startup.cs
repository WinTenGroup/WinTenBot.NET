using Exceptionless;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinTenDev.Zizi.Host.Tools;
using WinTenDev.Zizi.Utils.Extensions;
using WinTenDev.Zizi.Host.Extensions;

namespace WinTenDev.Zizi.Host
{
    public class Startup
    {
        private IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;

            Init.RunAll();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddZiziBot();
            services.AddMappingConfiguration();

            // services.Scan(selector =>
            // {
            //     selector.FromCallingAssembly()
            //         .FromApplicationDependencies(assembly =>
            //         {
            //             var assemblyName = assembly.FullName;
            //             return assemblyName.Contains("WinTenDev");
            //         })
            //         .AddClasses(filter => filter.Where(type =>
            //             {
            //                 var fullName = type.FullName;
            //                 var isType = (fullName.Contains("Handlers")
            //                               || fullName.Contains("Services")
            //                               // || !fullName.Contains("TelegramService")
            //                               || !fullName.Contains("DataContexts"))
            //                              && !fullName.Contains("Migrations")
            //                              && !fullName.Contains("Providers");
            //
            //                 return isType;
            //             })
            //         )
            //         .AsSelf()
            //         .WithScopedLifetime();
            // });


            services.AddHealthChecks();

            // services
            //     .AddTransient<ZiziBot>()
            //     .Configure<BotOptions<ZiziBot>>(Configuration.GetSection("ZiziBot"))
            //     .Configure<CustomBotOptions<ZiziBot>>(Configuration.GetSection("ZiziBot"));

            services.AddExceptionless();
            services.AddHttpContextAccessor();

            services.AddFluentMigration();
            services.AddEasyCachingDisk();

            services.AddSqlKataMysql();
            services.AddClickHouse();
            services.AddLiteDb();

            services.AddFeatureServices();

            services.AddCallbackQueryHandlers();
            services.AddGeneralEvents();
            services.AddGroupEvents();

            services.AddAdditionalCommands();
            services.AddCoreCommands();
            services.AddGBanCommands();
            services.AddGroupCommands();
            services.AddKataCommands();
            services.AddLearningCommands();
            services.AddNotesCommands();
            services.AddRssCommands();
            services.AddTagsCommands();
            services.AddWelcomeCommands();

            services.AddLocalTunnelClient();

            services.AddHangfireServerAndConfig();

            Log.Information("Services is ready..");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.SetupSerilog();
            app.AboutApp();

            app.UseFluentMigration();
            app.ConfigureNewtonsoftJson();
            app.ConfigureDapper();
            app.UseMonkeyCache();

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseStaticFiles();

            app.UseExceptionless();

            app.RunZiziBot();

            app.UseHangfireDashboardAndServer();

            app.Run(async context => await context.Response.WriteAsync("Hello World!"));

            app.UseEndpoints(endpoints => { endpoints.MapHealthChecks("/health"); });

            Log.Information("App is ready..");
        }
    }
}