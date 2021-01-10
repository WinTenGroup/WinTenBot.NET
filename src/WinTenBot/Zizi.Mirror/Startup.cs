using System;
using Google.Apis.Auth.OAuth2;
using LiteDB.Async;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Framework;
using Zizi.Core.Utils.GoogleCloud;
using Zizi.Mirror.Bots;
using Zizi.Mirror.Extensions;
using Zizi.Mirror.Handlers;
using Zizi.Mirror.Handlers.Commands;

namespace Zizi.Mirror
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            //GoogleServiceAccount.GenerateServiceAccount(2);
            //GoogleServiceAccount.ListServiceAccounts("zizibot-295007");

            Configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMapConfiguration(Configuration);

            services
                .AddTransient<ZiziMirror>()
                .Configure<BotOptions<ZiziMirror>>(Configuration.GetSection(nameof(ZiziMirror)));

            services.AddScoped(_ => new LiteDatabaseAsync("Filename=Storage/Data/Local_LiteDB.db;Connection=shared;"));

            services.AddScoped<ExceptionHandler>();
            services.AddScoped<NewUpdateHandler>();

            // services.AddScoped<AllDebridCommand>();
            // services.AddScoped<FastDebridCommand>();
            services.AddScoped<AuthorizeCommand>();
            //services.AddScoped<CloneCommand>();

            services.AddGoogleDrive();

            services.AddHangfireServerAndConfig();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var botBuilder = CommandBuilderExtension.ConfigureBot();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // else
            // {
            // app.UseTelegramBotWebhook<ZiziMirror>(botBuilder);
            // }

            app.UseTelegramLongPolling<ZiziMirror>(botBuilder, TimeSpan.FromSeconds(2));

            app.UseHangfireDashboardAndServer();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); }); });
        }
    }
}