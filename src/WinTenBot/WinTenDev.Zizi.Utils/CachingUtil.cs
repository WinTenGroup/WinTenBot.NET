using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using WinTenDev.Zizi.Utils.IO;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Utils
{
    public static class CachingUtil
    {
        private static readonly string workingDir = "Storage/Caches";

        public static async Task WriteCacheAsync(this object data, string fileJson, bool indented = true)
        {
            var filePath = $"{workingDir}/{fileJson}".EnsureDirectory();
            var json = data.ToJson(indented);

            await json.ToFile(filePath);
            Log.Information("Writing cache success..");
        }

        public static async Task<T> ReadCacheAsync<T>(this string fileJson)
        {
            var filePath = $"{workingDir}/{fileJson}";
            var json = await File.ReadAllTextAsync(filePath);
            var dataTable = json.MapObject<T>();

            Log.Information("Loaded cache items: {0}", fileJson);
            return dataTable;
        }

        public static bool IsFileCacheExist(this string fileName)
        {
            var filePath = $"{workingDir}/{fileName}";
            var isExist = File.Exists(filePath);
            Log.Information("IsCache {FileName} Exist: {IsExist}", fileName, isExist);

            return isExist;
        }

        public static void ClearCache(string keyword)
        {
            Log.Information("Deleting caches. Keyword {Keyword}", keyword);

            var listFile = Directory.GetFiles(workingDir);
            var listFiltered = listFile.Where(file =>
                file.Contains(keyword)).ToArray();

            Log.Information("Found cache target {Length} of {Length1}", listFiltered.Length, listFile.Length);
            foreach (var file in listFiltered)
            {
                Log.Information("Deleting {File}", file);
                File.Delete(file);
            }
        }

        public static void ClearCacheOlderThan(string keyword, int days = 1)
        {
            Log.Information("Deleting caches older than {Days} days", days);

            var dirInfo = new DirectoryInfo(workingDir);
            var files = dirInfo.GetFiles();
            var filteredFiles = files.Where(fileInfo =>
                fileInfo.CreationTimeUtc < DateTime.UtcNow.AddDays(-days) &&
                fileInfo.Name.Contains(keyword)).ToArray();

            Log.Information("Found cache target {Length} of {Length1}", filteredFiles.Length, files.Length);

            foreach (FileInfo file in filteredFiles)
            {
                Log.Information("Deleting {FullName}", file.FullName);
                File.Delete(file.FullName);
            }
        }
    }
}