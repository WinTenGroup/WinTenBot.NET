using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Models;
using WinTenDev.Zizi.Models.Bots;
using WinTenDev.Zizi.Utils.Providers;

namespace WinTenDev.Mirror.Host.Extensions
{
    public static class AppStartupExtension
    {

        public static IServiceCollection AddZiziBot(this IServiceCollection services)
        {
            var scope = services.BuildServiceProvider();
            var configuration = scope.GetRequiredService<IConfiguration>();

            services
                .AddTransient<ZiziMirror>()
                .Configure<BotOptions<ZiziMirror>>(configuration.GetSection(nameof(ZiziMirror)))
                .Configure<CustomBotOptions<ZiziMirror>>(configuration.GetSection(nameof(ZiziMirror)));

            // services.AddScoped(service =>
            // {
            //     var botToken = configuration.GetValue<string>(nameof(ZiziMirror) + ":Token");
            //     return new TelegramBotClient(botToken);
            // });

            return services;
        }

        public static IApplicationBuilder UseTelegramLongPolling<TBot>(
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

            Task.Run(async () =>
            {
                await Task.Delay(startAfter, cancellationToken);
                await updateManager.RunAsync(cancellationToken: cancellationToken);
                
            }, cancellationToken).ContinueWith(task =>
            {
                Log.Error(task.Exception, "Error LongPolling Bot");

                throw task.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);

            return app;
        }
        
        public static IApplicationBuilder EnsureWebhookSet<TBot>(
            this IApplicationBuilder app
        )
            where TBot : IBot
        {
            using var scope = app.ApplicationServices.CreateScope();
            
            var bot = scope.ServiceProvider.GetRequiredService<TBot>();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<CustomBotOptions<TBot>>>();
            var botToken = options.Value.ApiToken;
            var webhookPath = options.Value.WebhookPath;

            var url = new Uri(new Uri(options.Value.WebhookDomain), $"{webhookPath}/{botToken}/webhook");
            Log.Information("Url WebHook: {0}", url);

            Log.Information("Setting WebHook for bot {0} to URL {1}", typeof(TBot).Name, url);

            bot.Client.SetWebhookAsync(url.AbsoluteUri)
                .GetAwaiter().GetResult();

            return app;
        }
    }
}