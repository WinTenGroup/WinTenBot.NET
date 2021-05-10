using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Models.Enums;
using WinTenDev.ZiziBot.Utils;
using Zizi.Bot.Bots;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Options;
using Zizi.Bot.Providers;

namespace Zizi.Bot.Extensions
{
    internal static class AppStartupExtensions
    {
        public static IApplicationBuilder UseTelegramBotLongPolling<TBot>(
            this IApplicationBuilder app,
            IBotBuilder botBuilder,
            TimeSpan startAfter = default,
            CancellationToken cancellationToken = default
        )
            where TBot : BotBase
        {
            if (startAfter == default) startAfter = TimeSpan.FromSeconds(2);

            var updateManager = new UpdatePollingManager<TBot>(botBuilder, new BotServiceProvider(app));

#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
            Task.Run(async () =>
                {
                    await Task.Delay(startAfter, cancellationToken);
                    await updateManager.RunAsync(cancellationToken: cancellationToken);
                }, cancellationToken)
                .ContinueWith(t =>
                {
                    Log.Error(t.Exception!.Demystify(), "Error Starting Bot.");

                    throw t.Exception;
                }, TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler

            return app;
        }

        public static IApplicationBuilder EnsureWebhookSet<TBot>(this IApplicationBuilder app) where TBot : IBot
        {
            using var scope = app.ApplicationServices.CreateScope();
            var bot = scope.ServiceProvider.GetRequiredService<TBot>();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<CustomBotOptions<TBot>>>();

            var currentWebhook = bot.Client.GetWebhookInfoAsync().Result;

            var botToken = options.Value.ApiToken;
            var webhookPath = options.Value.WebhookPath;

            var url = new Uri(new Uri(options.Value.WebhookDomain), $"{webhookPath}/{botToken}/webhook");

            if (url.AbsoluteUri == currentWebhook.Url)
            {
                Log.Information("The webhook set is skipped because the WebHook is already set..");
                return app;
            }

            Log.Information("Url WebHook: {0}", url);
            Log.Information("Setting WebHook for bot {0}", typeof(TBot).Name);

            bot.Client.SetWebhookAsync(url.AbsoluteUri).GetAwaiter().GetResult();

            return app;
        }

        public static IApplicationBuilder AboutApp(this IApplicationBuilder app)
        {
            var engines = app.GetServiceProvider().GetRequiredService<IOptionsSnapshot<EnginesConfig>>().Value;

            Log.Information("Name: {ProductName}", engines.ProductName);
            Log.Information("Version: {Version}", engines.Version);
            Log.Information("Company: {Company}", engines.Company);

            return app;
        }

        public static IApplicationBuilder RunZiziBot(this IApplicationBuilder app)
        {
            Log.Information("Starting run ZiziBot..");
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var serviceProvider = app.GetServiceProvider();
            var commonConfig = serviceProvider.GetRequiredService<IOptionsSnapshot<CommonConfig>>().Value;

            switch (commonConfig.EngineMode)
            {
                case EngineMode.Polling:
                    app.RunInPooling();
                    break;
                case EngineMode.WebHook:
                    app.RunInWebHook();
                    break;
                case EngineMode.FollowHost:
                {
                    if (env.IsDevelopment())
                    {
                        app.RunInPooling();
                    }
                    else
                    {
                        app.RunInWebHook();
                    }

                    break;
                }
                default:
                    Log.Error("Unknown Engine Mode!");
                    break;
            }

            return app;
        }

        private static IApplicationBuilder RunInPooling(this IApplicationBuilder app)
        {
            var configureBot = CommandBuilderExtension.ConfigureBot();

            Log.Information("Starting ZiziBot in Pooling mode..");

            // get bot updates from Telegram via long-polling approach during development
            // this will disable Telegram webhooks
            app.UseTelegramBotLongPolling<ZiziBot>(configureBot, TimeSpan.FromSeconds(1));

            Log.Information("ZiziBot is ready in Pooling mode..");

            return app;
        }

        private static IApplicationBuilder RunInWebHook(this IApplicationBuilder app)
        {
            var configureBot = CommandBuilderExtension.ConfigureBot();

            Log.Information("Starting ZiziBot in WebHook mode..");
            // use Telegram bot webhook middleware in higher environments
            app.UseTelegramBotWebhook<ZiziBot>(configureBot);

            // and make sure webhook is enabled
            app.EnsureWebhookSet<ZiziBot>();

            Log.Information("ZiziBot is ready in WebHook mode..");

            return app;
        }
    }
}