using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types.InputFiles;
using WinTenBot.Model;

namespace WinTenBot.IO
{
    public static class Storage
    {
        public static async Task ClearLog()
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
                var filteredFiles = files.Where(fileInfo =>
                    fileInfo.CreationTimeUtc < DateTime.UtcNow.AddDays(-1)).ToArray();

                if (filteredFiles.Length > 0)
                {
                    Log.Information("Found {0} of {1}", filteredFiles.Length, files.Length);
                    foreach (var fileInfo in filteredFiles)
                    {
                        var filePath = fileInfo.FullName;
                        var zipFile = filePath.CreateZip();
                        Log.Information("Uploading file {0}", zipFile);
                        await using var fileStream = File.OpenRead(zipFile);

                        var media = new InputOnlineFile(fileStream, zipFile);
                        await botClient.SendDocumentAsync(channelTarget, media)
                            .ConfigureAwait(false);

                        fileStream.Close();
                        await fileStream.DisposeAsync().ConfigureAwait(false);

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

        public static async Task ClearLogs(this string logsPath, string filter = "", bool upload = true)
        {
            try
            {
                Log.Information($"Clearing {logsPath}, filter: {filter}, upload: {upload}");
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
                    fileInfo.FullName.Contains(filter)).ToArray();

                if (filteredFiles.Length > 0)
                {
                    Log.Information($"Found {filteredFiles.Length} of {files.Length}");
                    foreach (var fileInfo in filteredFiles)
                    {
                        var filePath = fileInfo.FullName;
                        if (upload)
                        {
                            Log.Information($"Uploading file {filePath}");
                            var fileStream = File.OpenRead(filePath);

                            var media = new InputOnlineFile(fileStream, fileInfo.Name);
                            await botClient.SendDocumentAsync(channelTarget, media)
                                .ConfigureAwait(false);

                            fileStream.Close();
                            await fileStream.DisposeAsync().ConfigureAwait(false);
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