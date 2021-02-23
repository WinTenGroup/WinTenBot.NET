using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Extensions;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Bot.Tools.GoogleCloud;

namespace Zizi.Bot.Tools
{
    public static class Init
    {
        public static void RunAll()
        {
            // DefaultTypeMap.MatchNamesWithUnderscores = true;
            // ConfigureNewtonsoftJson();

            BotSettings.FillSettings();
            // SerilogServiceExtension.SetupLogger();

            // Log.Information("Name: {0}", BotSettings.ProductName);
            // Log.Information("Version: {0}", BotSettings.ProductVersion);

            LiteDbProvider.InitializeLiteDb();
            // RavenDbProvider.InitDatabase();
            // MonkeyCacheUtil.SetupCache();

            BotSettings.DbConnectionString = BotSettings.DbConnectionString;
            // DbMigration.ConnectionString = BotSettings.DbConnectionString;


            // DbMigration.RunMySqlMigration();
            // SqlMigration.MigrateAll();

            GoogleDrive.Auth();

            // Bot.Clients.Add("macosbot", new TelegramBotClient(Configuration["MacOsBot:ApiToken"]));
            // Bot.Clients.Add("zizibot", new TelegramBotClient(Configuration["ZiziBot:ApiToken"]));
        }

        // private static void SetupHangfire()
        // {
        //     GlobalConfiguration.Configuration
        //         .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        //         .UseSimpleAssemblyNameTypeSerializer()
        //         .UseRecommendedSerializerSettings()
        //         .UseStorage(HangfireJobs.GetMysqlStorage())
        //         .UseSerilogLogProvider()
        //         .UseColouredConsoleLogProvider();
        // }

        // private static void ConfigureNewtonsoftJson()
        // {
        //     var contractResolver = new DefaultContractResolver()
        //     {
        //         NamingStrategy = new SnakeCaseNamingStrategy()
        //     };
        //
        //     JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
        //     {
        //         DateTimeZoneHandling = DateTimeZoneHandling.Local,
        //         ContractResolver = contractResolver,
        //         ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //     };
        // }
    }
}