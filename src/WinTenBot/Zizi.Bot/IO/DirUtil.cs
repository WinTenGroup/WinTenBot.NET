using System;
using System.IO;
using System.Linq;
using Serilog;
using Zizi.Bot.Common;

namespace Zizi.Bot.IO
{
    public static class DirUtil
    {
        public static string EnsureDirectory(this string dirPath, bool isDir = false)
        {
            Log.Debug("EnsuringDir: {0}", dirPath);

            var path = Path.GetDirectoryName(dirPath);
            if (isDir)
            {
                path = dirPath;
            }

            if (path.IsNullOrEmpty()) return dirPath;
            if (Directory.Exists(path)) return dirPath;

            Log.Debug("Creating directory {0}..", path);
            Directory.CreateDirectory(path);

            return dirPath;
        }

        public static long DirSize(this string path)
        {
            long size = 0;

            var d = new DirectoryInfo(path);
            // Add file sizes.
            var fis = d.GetFiles();
            foreach (var fi in fis)
            {
                size += fi.Length;
            }

            // Add subdirectory sizes.
            var dis = d.GetDirectories();
            foreach (var unused in dis)
            {
                size += DirSize(unused.FullName);
            }

            Log.Information("{Path} size is {Size}", path, size);
            return size;
        }

        public static string SanitizeSlash(this string path)
        {
            return path.Replace(@"\", "/", StringComparison.CurrentCulture)
                .Replace("\\", "/", StringComparison.CurrentCulture);
        }

        public static string GetDirectory(this string path)
        {
            return Path.GetDirectoryName(path) ?? path;
        }

        public static void RemoveFiles(this string path, string filter)
        {
            Log.Information("Deleting files in {Path}", path);
            var files = Directory.GetFiles(path)
                .Where(file => file.Contains(filter, StringComparison.CurrentCulture));

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public static bool IsDirectory(this string path)
        {
            var fa = File.GetAttributes(path);
            return (fa & FileAttributes.Directory) != 0;
        }
    }
}