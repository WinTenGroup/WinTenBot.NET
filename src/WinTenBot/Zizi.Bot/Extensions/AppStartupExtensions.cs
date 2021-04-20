﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
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
            if (startAfter == default)
            {
                startAfter = TimeSpan.FromSeconds(2);
            }

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
                Log.Information("The webhook set is skipped because the WebHook is already set.");
                return app;
            }

            Log.Information("Url WebHook: {0}", url);
            Log.Information("Setting WebHook for bot {0}", typeof(TBot).Name);

            bot.Client.SetWebhookAsync(url.AbsoluteUri).GetAwaiter().GetResult();

            return app;
        }

        public static IApplicationBuilder AboutApp(this IApplicationBuilder app)
        {
            var appConfig = app.ApplicationServices.GetRequiredService<AppConfig>();
            var engines = appConfig.EnginesConfig;

            Log.Information("Name: {0}", engines.ProductName);
            Log.Information("Version: {0}", engines.Version);
            Log.Information("Company: {0}", engines.Company);

            return app;
        }
    }
}