using System;
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Bots;
using WinTenBot.Common;
using WinTenBot.Extensions;
using WinTenBot.Handlers;
using WinTenBot.Handlers.Commands.Additional;
using WinTenBot.Handlers.Commands.Chat;
using WinTenBot.Handlers.Commands.Core;
using WinTenBot.Handlers.Commands.GlobalBan;
using WinTenBot.Handlers.Commands.Group;
using WinTenBot.Handlers.Commands.Notes;
using WinTenBot.Handlers.Commands.Rss;
using WinTenBot.Handlers.Commands.Rules;
using WinTenBot.Handlers.Commands.SpamLearning;
using WinTenBot.Handlers.Commands.Tags;
using WinTenBot.Handlers.Commands.Welcome;
using WinTenBot.Handlers.Commands.Words;
using WinTenBot.Handlers.Events;
using WinTenBot.Interfaces;
using WinTenBot.Model;
using WinTenBot.Options;
using WinTenBot.Providers;
using WinTenBot.Scheduler;
using WinTenBot.Services;
using WinTenBot.Tools;

namespace WinTenBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            BotSettings.GlobalConfiguration = Configuration;
            BotSettings.HostingEnvironment = env;

            BotSettings.Client = new TelegramBotClient(Configuration["ZiziBot:ApiToken"]);

            Init.RunAll();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddTransient<ZiziBot>()
                .Configure<BotOptions<ZiziBot>>(Configuration.GetSection("ZiziBot"))
                .Configure<CustomBotOptions<ZiziBot>>(Configuration.GetSection("ZiziBot"))
                .AddScoped<NewUpdateHandler>()
                .AddScoped<GenericMessageHandler>()
                .AddScoped<WebhookLogger>()
                .AddScoped<StickerHandler>()
                .AddScoped<WeatherReporter>()
                .AddScoped<ExceptionHandler>()
                .AddScoped<UpdateMembersList>()
                .AddScoped<CallbackQueryHandler>()
                .AddScoped<IWeatherService, WeatherService>();

            services.AddScoped<GlobalBanCommand>()
                .AddScoped<DeleteBanCommand>();

            services.AddScoped<AddKataCommand>()
                .AddScoped<WordSyncCommand>();

            services.AddScoped<PingHandler>()
                .AddScoped<HelpCommand>()
                .AddScoped<TestCommand>();

            services.AddScoped<MediaReceivedHandler>();

            services.AddScoped<MigrateCommand>()
                .AddScoped<MediaFilterCommand>();

            services.AddScoped<TagsCommand>()
                .AddScoped<TagCommand>()
                .AddScoped<UntagCommand>();

            services.AddScoped<NotesCommand>()
                .AddScoped<AddNotesCommand>();

            services.AddScoped<SetRssCommand>()
                .AddScoped<DelRssCommand>()
                .AddScoped<RssInfoCommand>()
                .AddScoped<RssPullCommand>()
                .AddScoped<RssCtlCommand>()
                .AddScoped<ExportRssCommand>()
                .AddScoped<ImportRssCommand>();

            services.AddScoped<AdminCommand>()
                .AddScoped<PinCommand>()
                .AddScoped<ReportCommand>()
                .AddScoped<AfkCommand>();

            services.AddScoped<KickCommand>()
                .AddScoped<BanCommand>()
                .AddScoped<WarnCommand>();

            services.AddScoped<PromoteCommand>()
                .AddScoped<DemoteCommand>();

            services.AddScoped<RulesCommand>();

            services.AddScoped<NewChatMembersEvent>()
                .AddScoped<LeftChatMemberEvent>()
                .AddScoped<PinnedMessageEvent>();

            services.AddScoped<WelcomeCommand>()
                .AddScoped<SetWelcomeCommand>()
                .AddScoped<WelcomeMessageCommand>()
                .AddScoped<WelcomeButtonCommand>()
                .AddScoped<WelcomeDocumentCommand>();

            services.AddScoped<SettingsCommand>()
                .AddScoped<ResetSettingsCommand>();

            services.AddScoped<PingCommand>()
                .AddScoped<DebugCommand>()
                .AddScoped<StartCommand>()
                .AddScoped<IdCommand>()
                .AddScoped<InfoCommand>()
                .AddScoped<BotCommand>()
                .AddScoped<GlobalReportCommand>();

            services.AddScoped<OutCommand>();

            services.AddScoped<QrCommand>()
                .AddScoped<OcrCommand>()
                .AddScoped<CatCommand>()
                .AddScoped<TranslateCommand>();

            services.AddScoped<CovidCommand>();

            services.AddScoped<LearnCommand>()
                .AddScoped<PredictCommand>()
                .AddScoped<ImportLearnCommand>();

            services.AddHangfireServer();
            services.AddHangfire(config =>
            {
                config.UseStorage(Tools.Hangfire.GetMysqlStorage());
                // config.UseStorage(Tools.Hangfire.GetSqliteStorage());
                // config.UseStorage(HangfireProvider.GetLiteDbStorage());

                config.UseSimpleAssemblyNameTypeSerializer()
                    .UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(10))
                    .UseSerilogLogProvider()
                    .UseRecommendedSerializerSettings();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            BotSettings.HostingEnvironment = env;

            var hangfireBaseUrl = Configuration["Hangfire:BaseUrl"];
            var hangfireUsername = Configuration["Hangfire:Username"];
            var hangfirePassword = Configuration["Hangfire:Password"];

            Log.Information($"Hangfire Auth: {hangfireUsername} | {hangfirePassword}");

            var dashboardOptions = new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangfireCustomBasicAuthenticationFilter {User = hangfireUsername, Pass = hangfirePassword}
                }
            };

            var configureBot = ConfigureBot();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // get bot updates from Telegram via long-polling approach during development
                // this will disable Telegram webhooks
                app.UseTelegramBotLongPolling<ZiziBot>(configureBot, TimeSpan.FromSeconds(1));
                app.UseHangfireDashboard("/hangfire", dashboardOptions);
            }
            else
            {
                // use Telegram bot webhook middleware in higher environments
                app.UseTelegramBotWebhook<ZiziBot>(configureBot);

                // and make sure webhook is enabled
                app.EnsureWebhookSet<ZiziBot>();
                app.UseHangfireDashboard(hangfireBaseUrl, dashboardOptions);
            }

            var serverOptions = new BackgroundJobServerOptions
            {
                WorkerCount = Environment.ProcessorCount * 2
            };

            app.UseHangfireServer(serverOptions, additionalProcesses: new[]
            {
                new ProcessMonitor(checkInterval: TimeSpan.FromSeconds(1))
            });

            app.Run(async context =>
                await context.Response.WriteAsync("Hello World!")
                    .ConfigureAwait(false));

            app.UseSerilogRequestLogging();

            BotScheduler.StartScheduler();
            
            Log.Information("App is ready.");
        }

        private IBotBuilder ConfigureBot()
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
                    .UseWhen<MediaReceivedHandler>(When.MediaReceived)
                    .UseWhen(When.NewOrEditedMessage, msgBranch => msgBranch
                        .UseWhen(When.NewTextMessage, txtBranch => txtBranch
                                .UseWhen<PingHandler>(When.PingReceived)
                                .UseWhen(When.NewCommand, cmdBranch => cmdBranch
                                    .UseCommand<AdminCommand>("admin")
                                    .UseCommand<AddKataCommand>("wfil")
                                    .UseCommand<AddKataCommand>("kata")
                                    .UseCommand<AddNotesCommand>("addfilter")
                                    .UseCommand<AfkCommand>("afk")
                                    .UseCommand<BanCommand>("ban")
                                    .UseCommand<BotCommand>("bot")
                                    .UseCommand<CatCommand>("cat")
                                    .UseCommand<CovidCommand>("covid")
                                    .UseCommand<DebugCommand>("dbg")
                                    .UseCommand<DeleteBanCommand>("dban")
                                    .UseCommand<DelRssCommand>("delrss")
                                    .UseCommand<DemoteCommand>("demote")
                                    .UseCommand<ExportRssCommand>("exportrss")
                                    .UseCommand<GlobalBanCommand>("fban")
                                    .UseCommand<GlobalBanCommand>("gban")
                                    .UseCommand<GlobalReportCommand>("greport")
                                    .UseCommand<HelpCommand>("help")
                                    .UseCommand<IdCommand>("id")
                                    .UseCommand<ImportLearnCommand>("importlearn")
                                    .UseCommand<ImportRssCommand>("importrss")
                                    .UseCommand<InfoCommand>("info")
                                    .UseCommand<KickCommand>("kick")
                                    .UseCommand<MediaFilterCommand>("mfil")
                                    .UseCommand<MigrateCommand>("migrate")
                                    .UseCommand<NotesCommand>("filters")
                                    .UseCommand<OutCommand>("out")
                                    .UseCommand<PinCommand>("pin")
                                    // .UseCommand<PingCommand>("ping")
                                    .UseCommand<PredictCommand>("predict")
                                    .UseCommand<PromoteCommand>("promote")
                                    .UseCommand<QrCommand>("qr")
                                    .UseCommand<ReportCommand>("report")
                                    .UseCommand<RssCtlCommand>("rssctl")
                                    .UseCommand<ResetSettingsCommand>("rsettings")
                                    .UseCommand<RssInfoCommand>("rssinfo")
                                    .UseCommand<RssPullCommand>("rsspull")
                                    .UseCommand<RulesCommand>("rules")
                                    .UseCommand<SetRssCommand>("setrss")
                                    .UseCommand<SettingsCommand>("settings")
                                    .UseCommand<SetWelcomeCommand>("setwelcome")
                                    .UseCommand<StartCommand>("start")
                                    .UseCommand<TagCommand>("tag")
                                    .UseCommand<TagsCommand>("notes")
                                    .UseCommand<TagsCommand>("tags")
                                    .UseCommand<LearnCommand>("learn")
                                    .UseCommand<OcrCommand>("ocr")
                                    .UseCommand<TestCommand>("test")
                                    .UseCommand<TranslateCommand>("tr")
                                    .UseCommand<UntagCommand>("untag")
                                    .UseCommand<WarnCommand>("warn")
                                    .UseCommand<WelcomeCommand>("welcome")
                                    .UseCommand<WelcomeMessageCommand>("welmsg")
                                    .UseCommand<WelcomeButtonCommand>("welbtn")
                                    .UseCommand<WelcomeDocumentCommand>("weldoc")
                                    .UseCommand<WordSyncCommand>("wsync")
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