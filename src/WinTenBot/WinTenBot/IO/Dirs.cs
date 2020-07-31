using System;
using System.IO;
using System.Linq;
using Serilog;
using WinTenBot.Common;

namespace WinTenBot.IO
{
    public static class Dirs
    {
        public static string EnsureDirectory(this string dirPath)
        {
            Log.Debug("EnsuringDir: {0}", dirPath);

            var path = Path.GetDirectoryName(dirPath);
            if (!path.IsNullOrEmpty())
            {
                Log.Debug("Creating directory..");
                Directory.CreateDirectory(path);
            }

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

            Log.Information($"{path} size is {size}");
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
            Log.Information($"Deleting files in {path}");
            var files = Directory.GetFiles(path)
                .Where(file => file.Contains(filter, StringComparison.CurrentCulture));

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}