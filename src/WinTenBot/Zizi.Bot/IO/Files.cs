using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Zizi.Bot.Models;

namespace Zizi.Bot.IO
{
    public static class Files
    {
        public static void DeleteFile(this string filePath)
        {
            if (!File.Exists(filePath)) return;

            try
            {
                Log.Information("Deleting {FilePath}", filePath);
                File.Delete(filePath);

                Log.Information("File {FilePath} deleted successfully", filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Deleting file {FilePath}", filePath);
            }
        }

        public static string ReplaceExt(this string fileName, string toExt)
        {
            // var fileExt = Path.GetExtension(fileName);
            // var newFile = fileName.Replace(fileExt,toExt);
            // return newFile;
            
            return Path.ChangeExtension(fileName, toExt);
        }

        public static async Task WriteTextAsync(this string content, string filePath)
        {
            var cachePath = BotSettings.PathCache;

            filePath = $"{cachePath}/{filePath}";
            Log.Information("Writing content to {FilePath}", filePath);

            Path.GetDirectoryName(filePath).EnsureDirectory();

            await File.WriteAllTextAsync(filePath, content)
                .ConfigureAwait(false);

            Log.Information("Writing file success..");
        }

        public static void WriteText(this string content, string filePath)
        {
            var cachePath = BotSettings.PathCache;

            filePath = $"{cachePath}/{filePath}";
            Log.Information("Writing content to {FilePath}", filePath);

            Path.GetDirectoryName(filePath).EnsureDirectory();

            File.WriteAllText(filePath, content);
            Log.Information("Writing file success..");
        }

        public static long FileSize(string filePath)
        {
            return new FileInfo(filePath).Length;
        }
        
        public static bool IsFileExist(this string filePath)
        {
            return File.Exists(filePath);
        }
    }
}