using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Zizi.Bot.Common;
using Zizi.Bot.IO;

namespace Zizi.Bot.Models
{
    [Obsolete("BotSettings will be replaced with AppConfig.")]
    public static class BotSettings
    {
        public static string ProductName { get; set; }
        public static string ProductVersion { get; set; }
        public static string ProductCompany { get; set; }
        public static ITelegramBotClient Client { get; set; }

        public static Dictionary<string, ITelegramBotClient> Clients { get; set; } =
            new Dictionary<string, ITelegramBotClient>();

        public static string DbConnectionString { get; set; }

        public static IConfiguration GlobalConfiguration { get; set; }

        public static IWebHostEnvironment HostingEnvironment { get; set; }
        public static bool IsDevelopment => HostingEnvironment.IsDevelopment();
        public static bool IsStaging => HostingEnvironment.IsStaging();
        public static bool IsProduction => HostingEnvironment.IsProduction();

        public static string PathCache { get; set; }
        public static string LearningDataSetPath { get; set; }

        public static List<string> Sudoers { get; set; }
        public static long BotChannelLogs { get; set; } = -1;
        public static string SpamWatchToken { get; set; }

        public static string GoogleZiziUploadUrl { get; set; }
        public static string GoogleCloudCredentialsPath { get; set; }
        public static string GoogleDriveAuth { get; set; }

        public static string UptoboxToken { get; set; }

        public static string HangfireBaseUrl { get; set; }
        public static string HangfireUsername { get; set; }
        public static string HangfirePassword { get; set; }
        public static string HangfireMysqlDb { get; set; }
        public static string HangfireSqliteDb { get; set; }
        public static string HangfireLiteDb { get; set; }

        public static string SerilogLogglyToken { get; set; }

        public static string DatadogApiKey { get; set; }
        public static string DatadogHost { get; set; }
        public static string DatadogSource { get; set; }
        public static List<string> DatadogTags { get; set; }

        public static string IbmWatsonTranslateUrl { get; set; }
        public static string IbmWatsonTranslateToken { get; set; }

        public static string TesseractTrainedData { get; set; }

        public static string OcrSpaceKey { get; set; }

        public static string RavenDBCertPath { get; set; }
        public static string RavenDBDatabase { get; set; }
        public static List<string> RavenDBNodes { get; set; }

        public static void FillSettings()
        {
            try
            {
                ProductName = GlobalConfiguration["EnginesConfig:ProductName"];
                ProductVersion = GlobalConfiguration["EnginesConfig:Version"];
                ProductCompany = GlobalConfiguration["EnginesConfig:Company"];

                Sudoers = GlobalConfiguration.GetSection("Sudoers").Get<List<string>>();
                BotChannelLogs = GlobalConfiguration["CommonConfig:ChannelLogs"].ToInt64();
                SpamWatchToken = GlobalConfiguration["CommonConfig:SpamWatchToken"];

                DbConnectionString = GlobalConfiguration["CommonConfig:ConnectionString"];

                GoogleZiziUploadUrl = GlobalConfiguration["GoogleCloudConfig:DriveIndexUrl"];
                GoogleCloudCredentialsPath = GlobalConfiguration["GoogleCloudConfig:CredentialsPath"];
                GoogleDriveAuth = GlobalConfiguration["GoogleCloudConfig:DriveAuth"];

                UptoboxToken = GlobalConfiguration["UptoboxConfig:Token"];

                HangfireBaseUrl = GlobalConfiguration["HangfireConfig:BaseUrl"];
                HangfireUsername = GlobalConfiguration["HangfireConfig:Username"];
                HangfirePassword = GlobalConfiguration["HangfireConfig:Password"];
                HangfireMysqlDb = GlobalConfiguration["HangfireConfig:MySql"];
                HangfireSqliteDb = GlobalConfiguration["HangfireConfig:Sqlite"];
                HangfireLiteDb = GlobalConfiguration["HangfireConfig:LiteDb"];

                SerilogLogglyToken = GlobalConfiguration["CommonConfig:LogglyToken"];

                DatadogApiKey = GlobalConfiguration["DatadogConfig:ApiKey"];
                DatadogHost = GlobalConfiguration["DatadogConfig:Host"];
                DatadogSource = GlobalConfiguration["DatadogConfig:Source"];
                DatadogTags = GlobalConfiguration.GetSection("DatadogConfig:Tags").Get<List<string>>();

                IbmWatsonTranslateUrl = GlobalConfiguration["IbmConfig:Watson:TranslateUrl"];
                IbmWatsonTranslateToken = GlobalConfiguration["IbmConfig:Watson:TranslateToken"];

                LearningDataSetPath = @"Storage\Learning\".EnsureDirectory();
                TesseractTrainedData = @"Storage\Data\Tesseract\";
                PathCache = "Storage/Caches";

                OcrSpaceKey = GlobalConfiguration["OcrSpaceConfig:ApiKey"];

                RavenDBCertPath = GlobalConfiguration["RavenDBConfig:CertPath"];
                RavenDBDatabase = GlobalConfiguration["RavenDBConfig:DBName"];
                RavenDBNodes = GlobalConfiguration.GetSection("RavenDBConfig:Nodes").Get<List<string>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error Loading Settings");
                Console.WriteLine($@"{ex.ToJson(true)}");
            }
        }

        public static bool IsEnvironment(string envName)
        {
            return HostingEnvironment.IsEnvironment(envName);
        }
    }
}