using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Mirror.Handlers;
using Zizi.Mirror.Handlers.Commands;

namespace Zizi.Mirror.Extensions
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
                    .UseCommand<CloneCommand>("clone")
                );
        }
    }
}