using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Handlers;
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
using Zizi.Bot.Handlers.Events;

namespace Zizi.Bot.Extensions
{
    public static class CommandBuilderExtension
    {
        public static IBotBuilder ConfigureBot()
        {
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
                        .UseWhen(When.NewTextMessage, txtBranch => txtBranch
                                .UseWhen<PingHandler>(When.PingReceived)
                                .UseWhen(When.NewCommand, cmdBranch => cmdBranch
                                        .UseCommand<AboutCommand>("about")
                                        .UseCommand<AddKataCommand>("kata")
                                        .UseCommand<AddKataCommand>("wfil")
                                        .UseCommand<AddNotesCommand>("addfilter")
                                        .UseCommand<AdminCommand>("admin")
                                        .UseCommand<AfkCommand>("afk")
                                        .UseCommand<AllDebridCommand>("ad")
                                        .UseCommand<BanCommand>("ban")
                                        .UseCommand<BotCommand>("bot")
                                        .UseCommand<CatCommand>("cat")
                                        .UseCommand<CheckResiCommand>("resi")
                                        .UseCommand<CovidCommand>("covid")
                                        .UseCommand<DebugCommand>("dbg")
                                        .UseCommand<DeleteBanCommand>("ungban")
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
                                        .UseCommand<MigrateCommand>("migrate")
                                        .UseCommand<NotesCommand>("filters")
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
                                        .UseCommand<WgetCommand>("wget")
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