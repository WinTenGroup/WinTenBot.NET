using System.IO;
using LiteDB;
using Serilog;
using WinTenBot.Common;
using WinTenBot.IO;

namespace WinTenBot.Providers
{
    public static class LiteDbProvider
    {
        private static LiteDatabase LiteDb { get; set; }

        public static void OpenDatabase(string dbName = "Local_LiteDB.db")
        {
            var fileName = Path.Combine("Storage", "Common", dbName).EnsureDirectory();
            var connBuild = new ConnectionString()
            {
                Filename = fileName,
                Upgrade = true,
                Connection = ConnectionType.Shared
            };

            LiteDb = new LiteDatabase(connBuild);
        }

        public static ILiteCollection<T> GetCollections<T>()
        {
            var collectionName = typeof(T).Name.Pluralize();
            Log.Information($"Getting collection {collectionName}");
            return LiteDb.GetCollection<T>(collectionName);
        }

        public static long Rebuild()
        {
            Log.Information("Rebuilding DB");
            return LiteDb.Rebuild();
        }
    }
}