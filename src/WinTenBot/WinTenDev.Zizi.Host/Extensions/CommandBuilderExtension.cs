using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Framework.UpdatePipeline;
using WinTenDev.Zizi.Host.Handlers;
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
using WinTenDev.Zizi.Host.Handlers.Events;

namespace WinTenDev.Zizi.Host.Extensions
{
    public static class CommandBuilderExtension
    {
        public static IBotBuilder ConfigureBot()
        {
            Log.Information("Building commands..");

            return new BotBuilder()
                    .Use<ExceptionHandler>()
                    // .Use<CustomUpdateLogger>()
                    .UseWhen<WebhookLogger>(When.Webhook)
                    .UseWhen<NewUpdateHandler>(When.NewUpdate)
                    //.UseWhen<UpdateMembersList>(When.MembersChanged)
                    .UseWhen<NewChatMembersEvent>(When.NewChatMembers)
                    .UseWhen<LeftChatMemberEvent>(When.LeftChatMember)

                    //.UseWhen(When.MembersChanged, memberChanged => memberChanged
                    //    .UseWhen(When.MembersChanged, cmdBranch => cmdBranch
                    //        .Use<NewChatMembersCommand>()
                    //        )
                    //    )
                    .UseWhen<PinnedMessageEvent>(When.NewPinnedMessage)
                    // .UseWhen<MediaReceivedHandler>(When.MediaReceived)
                    .UseWhen(When.NewOrEditedMessage, msgBranch => msgBranch
                        .UseWhen(When.CallTagReceived, tagBranch => tagBranch
                            .Use<FindTagCommand>())
                        .UseWhen(When.NewTextMessage, txtBranch => txtBranch
                                .UseWhen<PingHandler>(When.PingReceived)
                                .UseWhen(When.NewCommand, cmdBranch => cmdBranch
                                        .UseCommand<AboutCommand>("about")
                                        .UseCommand<AddBlockListCommand>("addblist")
                                        .UseCommand<AddKataCommand>("kata")
                                        .UseCommand<AddKataCommand>("wfil")
                                        .UseCommand<AddNotesCommand>("addfilter")
                                        .UseCommand<AdminCommand>("admin")
                                        .UseCommand<AdminCommand>("adminlist")
                                        .UseCommand<AfkCommand>("afk")
                                        .UseCommand<AllDebridCommand>("ad")
                                        .UseCommand<BackupCommand>("backup")
                                        .UseCommand<BanCommand>("ban")
                                        .UseCommand<BotCommand>("bot")
                                        .UseCommand<CatCommand>("cat")
                                        .UseCommand<CheckResiCommand>("resi")
                                        .UseCommand<CovidCommand>("covid")
                                        .UseCommand<DebugCommand>("dbg")
                                        .UseCommand<DeleteBanCommand>("ungban")
                                        .UseCommand<DeleteBanCommand>("dban")
                                        .UseCommand<DelRssCommand>("delrss")
                                        .UseCommand<DemoteCommand>("demote")
                                        .UseCommand<ExportRssCommand>("exportrss")
                                        .UseCommand<GBanRegisterCommand>("gbanreg")
                                        .UseCommand<GlobalBanCommand>("fban")
                                        .UseCommand<GlobalBanCommand>("gban")
                                        .UseCommand<GlobalReportCommand>("greport")
                                        .UseCommand<GlobalBanSyncCommand>("gbansync")
                                        .UseCommand<HelpCommand>("help")
                                        .UseCommand<IdCommand>("id")
                                        .UseCommand<ImportLearnCommand>("importlearn")
                                        .UseCommand<ImportRssCommand>("importrss")
                                        .UseCommand<KataSyncCommand>("ksync")
                                        .UseCommand<KickCommand>("kick")
                                        .UseCommand<LearnCommand>("learn")
                                        .UseCommand<MediaFilterCommand>("mfil")
                                        .UseCommand<NotesCommand>("filters")
                                        .UseCommand<NudityCommand>("nudity")
                                        .UseCommand<OcrCommand>("ocr")
                                        .UseCommand<OutCommand>("out")
                                        .UseCommand<PinCommand>("pin")
                                        .UseCommand<PredictCommand>("predict")
                                        .UseCommand<PromoteCommand>("promote")
                                        .UseCommand<QrCommand>("qr")
                                        .UseCommand<ReportCommand>("report")
                                        .UseCommand<ResetSettingsCommand>("rsettings")
                                        .UseCommand<RssCtlCommand>("rssctl")
                                        .UseCommand<RssInfoCommand>("rssinfo")
                                        .UseCommand<RssPullCommand>("rsspull")
                                        .UseCommand<RulesCommand>("rules")
                                        .UseCommand<SetRssCommand>("addrss")
                                        .UseCommand<SetRssCommand>("setrss")
                                        .UseCommand<SettingsCommand>("settings")
                                        .UseCommand<SetWelcomeCommand>("setwelcome")
                                        .UseCommand<StartCommand>("start")
                                        .UseCommand<StatsCommand>("stats")
                                        .UseCommand<StickerPackCommand>("stickerpack")
                                        .UseCommand<StorageCommand>("storage")
                                        .UseCommand<TagCommand>("tag")
                                        .UseCommand<TagsCommand>("notes")
                                        .UseCommand<TagsCommand>("tags")
                                        .UseCommand<TestCommand>("test")
                                        .UseCommand<TranslateCommand>("tr")
                                        .UseCommand<UntagCommand>("untag")
                                        .UseCommand<UsernameCommand>("username")
                                        .UseCommand<WarnCommand>("warn")
                                        .UseCommand<WelcomeButtonCommand>("welbtn")
                                        .UseCommand<WelcomeCommand>("welcome")
                                        .UseCommand<WelcomeDocumentCommand>("weldoc")
                                        .UseCommand<WelcomeMessageCommand>("welmsg")
                                    // .UseCommand<WgetCommand>("wget")
                                    // .UseCommand<PingCommand>("ping")
                                )
                                .Use<GenericMessageHandler>()

                            //.Use<NLP>()
                        )
                        // .UseWhen<StickerHandler>(When.StickerMessage)
                        .UseWhen<WeatherReporter>(When.LocationMessage)
                    )
                    .UseWhen<CallbackQueryHandler>(When.CallbackQuery)

                //.Use<UnhandledUpdateReporter>()
                ;
        }
    }
}