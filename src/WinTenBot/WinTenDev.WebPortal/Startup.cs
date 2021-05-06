using System.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using MySqlConnector.Logging;
using Zizi.Core.Extensions;
using Zizi.Portal.Data;

namespace Zizi.Portal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connString = Configuration.GetConnectionString("MysqlDatabase");

            services.AddRazorPages();
            services.AddBootstrapBlazor();
            services.AddServerSideBlazor();
            services.AddHttpClient();

            services.AddEasyCachingDisk();
            services.AddSqlKataMysql(connString);
            services.AddSingleton<WeatherForecastService>();

            services.AddScoped<IDbConnection>(_ => new MySqlConnection(connString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // var factory = app.ApplicationServices.GetService<ILoggerFactory>();
            MySqlConnectorLogManager.Provider = new SerilogLoggerProvider();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}