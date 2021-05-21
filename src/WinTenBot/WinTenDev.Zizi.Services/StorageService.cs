using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types.InputFiles;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Utils.IO;

namespace WinTenDev.Zizi.Services
{
    public class StorageService
    {
        public async Task ClearLog()
        {
            try
            {
                const string logsPath = "Storage/Logs";
                var botClient = BotSettings.Client;
                var channelTarget = BotSettings.BotChannelLogs;

                if (!channelTarget.ToString().StartsWith("-100"))
                {
                    Log.Information("Please specify ChannelTarget in appsettings.json");
                    return;
                }

                var dirInfo = new DirectoryInfo(logsPath);
                var files = dirInfo.GetFiles();
                // var filteredCreated = files.Where(fileInfo => fileInfo.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1)).ToList();
                // var filteredModified = files.Where(fileInfo => fileInfo.CreationTimeUtc < DateTime.UtcNow.AddDays(-1)).ToList();

                var filteredFile = files.Where(fileInfo =>
                    fileInfo.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1)
                    || fileInfo.CreationTimeUtc < DateTime.UtcNow.AddDays(-1)
                ).ToList();

                // filteredCreated.AddRange(filteredModified);

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

                        await botClient.SendDocumentAsync(channelTarget, media);

                        fileStream.Close();
                        await fileStream.DisposeAsync();

                        filePath.DeleteFile();
                        zipFile.DeleteFile();
                    }
                }
                else
                {
                    Log.Information("No Logs file need be processed for previous date");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Send .Log file to ChannelTarget.");
            }
        }

        public async Task ClearLogs(string logsPath, string filter = "", bool upload = true)
        {
            try
            {
                Log.Information("Clearing {LogsPath}, filter: {Filter}, upload: {Upload}", logsPath, filter, upload);
                var botClient = BotSettings.Client;
                var channelTarget = BotSettings.BotChannelLogs;

                if (!channelTarget.ToString().StartsWith("-100"))
                {
                    Log.Information("Please specify ChannelTarget in appsettings.json");
                    return;
                }

                var dirInfo = new DirectoryInfo(logsPath);
                var files = dirInfo.GetFiles();
                var filteredFiles = files.Where(fileInfo =>
                    fileInfo.CreationTimeUtc < DateTime.UtcNow.AddDays(-1) &&
                    fileInfo.FullName.Contains(filter)).ToList();

                // filteredCreated.AddRange(filteredModified);

                var filteredFilesCount = filteredFiles.Count;

                if (filteredFilesCount > 0)
                {
                    Log.Information("Found {Length} of {Length1}", filteredFilesCount, files.Length);
                    foreach (var fileInfo in filteredFiles)
                    {
                        var filePath = fileInfo.FullName;
                        if (upload)
                        {
                            Log.Information("Uploading file {FilePath}", filePath);
                            var fileStream = File.OpenRead(filePath);

                            var media = new InputOnlineFile(fileStream, fileInfo.Name);
                            await botClient.SendDocumentAsync(channelTarget, media);

                            fileStream.Close();
                            await fileStream.DisposeAsync();
                        }

                        var old = filePath + ".old";
                        File.Move(filePath, old);
                        old.DeleteFile();
                    }
                }
                else
                {
                    Log.Information("No Logs file need be processed for previous date");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Send .Log file to ChannelTarget.");
            }
        }
    }
}