using System.IO;
using MonkeyCache;
using MonkeyCache.FileStore;
using Serilog;

namespace WinTenDev.WebApi.Host.Helpers
{
    public class MonkeyHelper
    {
        public static void SetupCache()
        {
            Log.Information("Initializing MonkeyCache");
            var cachePath = Path.Combine("Storage", "MonkeyCache").EnsureDir();
            Barrel.ApplicationId = "WinTenApi-Cache";
            BarrelUtils.SetBaseCachePath(cachePath);

            Log.Debug("MonkeyCache initialized.");
        }
    }
}