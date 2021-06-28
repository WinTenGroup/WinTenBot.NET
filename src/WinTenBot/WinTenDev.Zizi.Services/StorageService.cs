using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Models.Enums;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.IO;

namespace WinTenDev.Zizi.Services
{
    /// <summary>
    /// Storage service for storage management
    /// </summary>
    public class StorageService
    {
        private readonly CommonConfig _commonConfig;
        private readonly TelegramBotClient _botClient;
        private readonly QueryService _queryService;

        /// <summary>
        /// Storage service constructor
        /// </summary>
        /// <param name="optionsCommonConfig"></param>
        /// <param name="botClient"></param>
        /// <param name="queryService"></param>
        public StorageService(
            IOptionsSnapshot<CommonConfig> optionsCommonConfig,
            TelegramBotClient botClient,
            QueryService queryService
        )
        {
            _commonConfig = optionsCommonConfig.Value;
            _botClient = botClient;
            _queryService = queryService;
        }

        /// <summary>
        /// Log management for delete old log and upload to channel
        /// </summary>
        public async Task ClearLog()
        {
            try
            {
                const string logsPath = "Storage/Logs";
                var channelTarget = _commonConfig.ChannelLogs;

                if (!channelTarget.StartsWith("-100"))
                {
                    Log.Information("Please specify ChannelTarget in appsettings.json");
                    return;
                }

                var dirInfo = new DirectoryInfo(logsPath);
                var files = dirInfo.GetFiles();

                var filteredFile = files.Where(fileInfo =>
                    fileInfo.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1)
                    || fileInfo.CreationTimeUtc < DateTime.UtcNow.AddDays(-1)
                ).ToList();

                var fileCount = filteredFile.Count;

                if (fileCount > 0)
                {
                    Log.Information("Found {FileCount} of {Length}", fileCount, files.Length);
                    foreach (var fileInfo in filteredFile)
                    {
                        var filePath = fileInfo.FullName;
                        var zipFile = filePath.CreateZip();
                        Log.Information("Uploading file {ZipFile}", zipFile);
                        await using var fileStream = File.OpenRead(zipFile);

                        var media = new InputOnlineFile(fileStream, zipFile)
                        {
                            FileName = Path.GetFileName(zipFile)
                        };

                        await _botClient.SendDocumentAsync(channelTarget, media);

                        fileStream.Close();
                        await fileStream.DisposeAsync();

                        filePath.DeleteFile();
                        zipFile.DeleteFile();

                        Log.Information("Upload file {ZipFile} succeeded", zipFile);
                    }
                }
                else
                {
                    Log.Information("No Logs file need be processed for previous date");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Send .Log file to ChannelTarget");
            }
        }

        /// <summary>
        /// Hangfire storage reset
        /// </summary>
        public async Task ResetHangfire(ResetTableMode resetTableMode = ResetTableMode.Truncate)
        {
            Log.Information("Starting reset Hangfire MySQL storage");

            const string prefixTable = "_hangfire";
            var factory = _queryService.CreateMySqlConnection();
            var sbSql = new StringBuilder();

            var listTable = new[]
            {
                "AggregatedCounter",
                "Counter",
                "DistributedLock",
                "Hash",
                "JobParameter",
                "JobQueue",
                "State",
                "List",
                "Server",
                "Set",
                "State",
                "Job",
            };

            sbSql.AppendLine("SET FOREIGN_KEY_CHECKS = 0;");

            foreach (var table in listTable)
            {
                var tableName = $"{prefixTable}{table}";
                var resetMode = resetTableMode.Humanize().ToUpperCase();

                sbSql.Append($"{resetMode} TABLE ");

                if (resetMode.Contains("drop", StringComparison.CurrentCultureIgnoreCase)) sbSql.Append("IF EXISTS ");

                sbSql.AppendLine($"{tableName};");

                // sbSql.AppendLine($"{resetMode} TABLE {tableName};");
            }

            sbSql.AppendLine("SET FOREIGN_KEY_CHECKS = 1;");

            var sqlTruncate = sbSql.ToTrimmedString();
            var rowCount = await factory.RunSqlAsync(sqlTruncate);

            Log.Information("Reset Hangfire MySQL storage finish. Result: {0}", rowCount);
        }
    }
}