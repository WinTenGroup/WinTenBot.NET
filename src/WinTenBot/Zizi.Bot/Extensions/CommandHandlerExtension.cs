using Microsoft.Extensions.DependencyInjection;
using Zizi.Bot.Handlers.Commands.Additional;
using Zizi.Bot.Handlers.Commands.Chat;
using Zizi.Bot.Handlers.Commands.Core;
using Zizi.Bot.Handlers.Commands.GlobalBan;
using Zizi.Bot.Handlers.Commands.Group;
using Zizi.Bot.Handlers.Commands.Metrics;
using Zizi.Bot.Handlers.Commands.Notes;
using Zizi.Bot.Handlers.Commands.Rss;
using Zizi.Bot.Handlers.Commands.Rules;
using Zizi.Bot.Handlers.Commands.SpamLearning;
using Zizi.Bot.Handlers.Commands.Tags;
using Zizi.Bot.Handlers.Commands.Welcome;
using Zizi.Bot.Handlers.Commands.Words;

namespace Zizi.Bot.Extensions
{
    public static class CommandHandlerExtension
    {
        public static IServiceCollection AddAdditionalCommands(this IServiceCollection services)
        {
            return services.AddScoped<QrCommand>()
                .AddScoped<OcrCommand>()
                .AddScoped<CatCommand>()
                .AddScoped<TranslateCommand>()
                .AddScoped<CovidCommand>()
                .AddScoped<WgetCommand>()
                .AddScoped<AllDebridCommand>()
                .AddScoped<CheckResiCommand>()
                .AddScoped<NudityCommand>()
                .AddScoped<StickerPackCommand>();
        }

        public static IServiceCollection AddCoreCommands(this IServiceCollection services)
        {
            return services.AddScoped<OutCommand>()
                .AddScoped<HelpCommand>()
                .AddScoped<TestCommand>()
                .AddScoped<UsernameCommand>()
                .AddScoped<StatsCommand>()
                .AddScoped<PingCommand>()
                .AddScoped<DebugCommand>()
                .AddScoped<StartCommand>()
                .AddScoped<IdCommand>()
                .AddScoped<AboutCommand>()
                .AddScoped<BotCommand>()
                .AddScoped<GlobalReportCommand>()
                .AddScoped<StorageCommand>()
                .AddScoped<SettingsCommand>()
                .AddScoped<ResetSettingsCommand>()
                .AddScoped<MigrateCommand>();
        }

        public static IServiceCollection AddGBanCommands(this IServiceCollection services)
        {
            return services.AddScoped<GlobalBanCommand>()
                .AddScoped<DeleteBanCommand>()
                .AddScoped<GlobalBanSyncCommand>()
                .AddScoped<GBanRegisterCommand>()
                .AddScoped<MediaFilterCommand>();
        }

        public static IServiceCollection AddGroupCommands(this IServiceCollection services)
        {
            return services.AddScoped<AdminCommand>()
                .AddScoped<PinCommand>()
                .AddScoped<ReportCommand>()
                .AddScoped<AfkCommand>()
                .AddScoped<KickCommand>()
                .AddScoped<BanCommand>()
                .AddScoped<WarnCommand>()
                .AddScoped<RulesCommand>()
                .AddScoped<PromoteCommand>()
                .AddScoped<DemoteCommand>();
        }

        public static IServiceCollection AddKataCommands(this IServiceCollection services)
        {
            return services.AddScoped<AddKataCommand>()
                    .AddScoped<KataSyncCommand>();
        }

        public static IServiceCollection AddLearningCommands(this IServiceCollection services)
        {
            return services.AddScoped<LearnCommand>()
                .AddScoped<PredictCommand>()
                .AddScoped<ImportLearnCommand>();
        }

        public static IServiceCollection AddNotesCommands(this IServiceCollection services)
        {
            return services.AddScoped<NotesCommand>()
                .AddScoped<AddNotesCommand>();
        }

        public static IServiceCollection AddRssCommands(this IServiceCollection services)
        {
            return services.AddScoped<SetRssCommand>()
                .AddScoped<DelRssCommand>()
                .AddScoped<RssInfoCommand>()
                .AddScoped<RssPullCommand>()
                .AddScoped<RssCtlCommand>()
                .AddScoped<ExportRssCommand>()
                .AddScoped<ImportRssCommand>();
        }

        public static IServiceCollection AddTagsCommands(this IServiceCollection services)
        {
            return services
                .AddScoped<FindTagCommand>()
                .AddScoped<TagsCommand>()
                .AddScoped<TagCommand>()
                .AddScoped<UntagCommand>();
        }

        public static IServiceCollection AddWelcomeCommands(this IServiceCollection services)
        {
            return services.AddScoped<WelcomeCommand>()
                .AddScoped<SetWelcomeCommand>()
                .AddScoped<WelcomeMessageCommand>()
                .AddScoped<WelcomeButtonCommand>()
                .AddScoped<WelcomeDocumentCommand>();
        }
    }
}