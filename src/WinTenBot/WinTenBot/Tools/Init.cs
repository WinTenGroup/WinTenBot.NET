using Dapper;
using Hangfire;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using WinTenBot.Common;
using WinTenBot.Common.JsonSettings;
using WinTenBot.Model;

namespace WinTenBot.Tools
{
    public static class Init
    {
        public static void RunAll()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            ConfigureNewtonsoftJson();
            BotSettings.FillSettings();
            Logger.SetupLogger();

            Log.Information($"Name: {BotSettings.ProductName}");
            Log.Information($"Version: {BotSettings.ProductVersion}");

            BotSettings.DbConnectionString = BotSettings.DbConnectionString;
            DbMigration.ConnectionString = BotSettings.DbConnectionString;


            DbMigration.RunMySqlMigration();
            SqlMigration.MigrateAll();

            // Bot.Clients.Add("macosbot", new TelegramBotClient(Configuration["MacOsBot:ApiToken"]));
            // Bot.Clients.Add("zizibot", new TelegramBotClient(Configuration["ZiziBot:ApiToken"]));

            SetupHangfire();
        }

        private static void SetupHangfire()
        {
            GlobalConfiguration.Configuration
                .UseSerilogLogProvider()
                .UseColouredConsoleLogProvider();
        }

        private static void ConfigureNewtonsoftJson()
        {
            var contractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                ContractResolver = contractResolver
            };
        }
    }
}