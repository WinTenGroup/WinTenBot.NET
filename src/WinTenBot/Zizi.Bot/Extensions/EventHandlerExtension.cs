using Microsoft.Extensions.DependencyInjection;
using Zizi.Bot.Handlers;
using Zizi.Bot.Handlers.Events;

namespace Zizi.Bot.Extensions
{
    public static class EventHandlerExtension
    {
        public static IServiceCollection AddGroupEvents(this IServiceCollection services)
        {
            return services.AddScoped<NewChatMembersEvent>()
                .AddScoped<LeftChatMemberEvent>()
                .AddScoped<PinnedMessageEvent>();
        }

        public static IServiceCollection AddGeneralEvents(this IServiceCollection services)
        {
            return services.AddScoped<NewUpdateHandler>()
                .AddScoped<GenericMessageHandler>()
                .AddScoped<WebhookLogger>()
                .AddScoped<StickerHandler>()
                .AddScoped<WeatherReporter>()
                .AddScoped<ExceptionHandler>()
                .AddScoped<UpdateMembersList>()
                .AddScoped<CallbackQueryHandler>()
                .AddScoped<PingHandler>();
            // .AddScoped<MediaReceivedHandler>();
        }
    }
}