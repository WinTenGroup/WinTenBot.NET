using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using Telegram.Bot;
using Telegram.Bot.Framework;
using Zizi.Bot.Bots;
using Zizi.Bot.Extensions;
using Zizi.Bot.Interfaces;
using Zizi.Bot.Models;
using Zizi.Bot.Options;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment Environment { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;

            BotSettings.GlobalConfiguration = Configuration;
            BotSettings.HostingEnvironment = env;

            BotSettings.Client = new TelegramBotClient(Configuration["ZiziBot:ApiToken"]);

            Init.RunAll();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddTransient<ZiziBot>()
                .Configure<BotOptions<ZiziBot>>(Configuration.GetSection("ZiziBot"))
                .Configure<CustomBotOptions<ZiziBot>>(Configuration.GetSection("ZiziBot"))
                .AddScoped<IWeatherService, WeatherService>();

            services.AddMapConfiguration(Configuration, Environment);

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

            services.AddHangfireServerAndConfig();

            Log.Information("Services is ready.");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var configureBot = CommandBuilderExtension.ConfigureBot();
            BotSettings.HostingEnvironment = env;

            app.UseSerilogRequestLogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // get bot updates from Telegram via long-polling approach during development
                // this will disable Telegram webhooks
                app.UseTelegramBotLongPolling<ZiziBot>(configureBot, TimeSpan.FromSeconds(1));
            }
            else
            {
                // use Telegram bot webhook middleware in higher environments
                app.UseTelegramBotWebhook<ZiziBot>(configureBot);

                // and make sure webhook is enabled
                app.EnsureWebhookSet<ZiziBot>();
            }

            app.UseHangfireDashboardAndServer();

            app.Run(async context =>
                await context.Response.WriteAsync("Hello World!")
                    .ConfigureAwait(false));

            Log.Information("App is ready.");
        }
    }
}