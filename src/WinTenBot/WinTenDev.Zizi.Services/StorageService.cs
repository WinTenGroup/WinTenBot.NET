using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Utils.IO;

namespace WinTenDev.Zizi.Services
{
    public class StorageService
    {
        private CommonConfig _commonConfig;
        private readonly TelegramBotClient _botClient;

        public StorageService(
            IOptionsSnapshot<CommonConfig> optionsCommonConfig,
            TelegramBotClient botClient
        )
        {
            _commonConfig = optionsCommonConfig.Value;
            _botClient = botClient;
        }

        public async Task ClearLog()
        {
            try
            {
                const string logsPath = "Storage/Logs";
                var channelTarget = _commonConfig.ChannelLogs;

                if (!channelTarget.ToString().StartsWith("-100"))
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
                        Log.Information("Uploading file {0}", zipFile);
                        await using var fileStream = File.OpenRead(zipFile);

                        var media = new InputOnlineFile(fileStream, zipFile);
                        media.FileName = Path.GetFileName(zipFile);

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
    }
}