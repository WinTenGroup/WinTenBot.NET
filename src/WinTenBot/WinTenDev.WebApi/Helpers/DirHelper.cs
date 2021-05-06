using System.IO;
using Serilog;

namespace WinTenDev.WebApi.Helpers
{
    public static class DirHelper
    {
        public static string EnsureDir(this string dirPath)
        {
            Log.Debug("EnsuringDir: {0}", dirPath);

            if (string.IsNullOrEmpty(dirPath)) return dirPath;
            if (Directory.Exists(dirPath)) return dirPath;

            Log.Debug("Creating directory {0}..", dirPath);
            Directory.CreateDirectory(dirPath);

            return dirPath;
        }
    }
}