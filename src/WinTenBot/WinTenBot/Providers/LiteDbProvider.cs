using System.IO;
using System.Threading.Tasks;
using Humanizer;
using LiteDB;
using Serilog;
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
            Log.Debug("Getting collection {0}", collectionName);
            return LiteDb.GetCollection<T>(collectionName);
        }

        public static async Task<ILiteCollection<T>> GetCollectionsAsync<T>()
        {
            var collectionName = typeof(T).Name.Pluralize();
            Log.Debug("Getting collection {0} async", collectionName);
            return await Task.Run(() => LiteDb.GetCollection<T>(collectionName))
                .ConfigureAwait(false);
        }

        public static long Rebuild()
        {
            Log.Information("Rebuilding DB");
            return LiteDb.Rebuild();
        }
    }
}