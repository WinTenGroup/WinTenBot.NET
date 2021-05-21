using Microsoft.Extensions.DependencyInjection;
using WinTenDev.Zizi.Host.Handlers.Commands.Additional;
using WinTenDev.Zizi.Host.Handlers.Commands.BlackLists;
using WinTenDev.Zizi.Host.Handlers.Commands.Chat;
using WinTenDev.Zizi.Host.Handlers.Commands.Core;
using WinTenDev.Zizi.Host.Handlers.Commands.GlobalBan;
using WinTenDev.Zizi.Host.Handlers.Commands.Group;
using WinTenDev.Zizi.Host.Handlers.Commands.Metrics;
using WinTenDev.Zizi.Host.Handlers.Commands.Notes;
using WinTenDev.Zizi.Host.Handlers.Commands.Rss;
using WinTenDev.Zizi.Host.Handlers.Commands.SpamLearning;
using WinTenDev.Zizi.Host.Handlers.Commands.Tags;
using WinTenDev.Zizi.Host.Handlers.Commands.Welcome;
using WinTenDev.Zizi.Host.Handlers.Commands.Words;

namespace WinTenDev.Zizi.Host.Extensions
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
                // .AddScoped<WgetCommand>()
                .AddScoped<AllDebridCommand>()
                .AddScoped<CheckResiCommand>()
                .AddScoped<NudityCommand>()
                .AddScoped<StickerPackCommand>()
                .AddScoped<AddBlockListCommand>();
        }

        public static IServiceCollection AddCoreCommands(this IServiceCollection services)
        {
            return services.AddScoped<OutCommand>()
                .AddScoped<BackupCommand>()
                .AddScoped<HelpCommand>()
                .AddScoped<TestCommand>()
                .AddScoped<UsernameCommand>()
                .AddScoped<StatsCommand>()
                .AddScoped<DebugCommand>()
                .AddScoped<StartCommand>()
                .AddScoped<IdCommand>()
                .AddScoped<AboutCommand>()
                .AddScoped<BotCommand>()
                .AddScoped<GlobalReportCommand>()
                .AddScoped<StorageCommand>()
                .AddScoped<SettingsCommand>()
                .AddScoped<ResetSettingsCommand>();
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