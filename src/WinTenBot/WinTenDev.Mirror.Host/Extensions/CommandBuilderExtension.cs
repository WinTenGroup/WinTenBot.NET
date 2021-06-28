using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Framework.UpdatePipeline;
using WinTenDev.Mirror.Host.Handlers;
using WinTenDev.Mirror.Host.Handlers.Commands;

namespace WinTenDev.Mirror.Host.Extensions
{
    public static class CommandBuilderExtension
    {
        public static IBotBuilder ConfigureBot()
        {
            return new BotBuilder()
                .Use<ExceptionHandler>()
                .UseWhen<NewUpdateHandler>(When.NewUpdate)
                .UseWhen(When.NewCommand, builder => builder
                    .UseCommand<AuthorizeCommand>("auth")
                    .UseCommand<UnAuthorizeAllCommand>("unauthall")
                    .UseCommand<UnAuthorizeCommand>("unauth")
                    // .UseCommand<AllDebridCommand>("ad")
                    // .UseCommand<FastDebridCommand>("fd")
                    .UseCommand<RDebridCommand>("rdb")
                    .UseCommand<CloneCommand>("clone")
                    .UseCommand<PingCommand>("ping")
                );
        }
    }
}