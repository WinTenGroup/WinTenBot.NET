using System.IO;
using System.Threading.Tasks;
using Humanizer;
using LiteDB;
using Serilog;
using Zizi.Bot.IO;

namespace Zizi.Bot.Providers
{
    public static class LiteDbProvider
    {
        private static LiteDatabase LiteDb { get; set; }

        private static string GetDbName(string dbName = "Local_LiteDB.db")
        {
            Log.Debug("Getting LiteDB FileName: {0}", dbName);
            var fileName = Path.Combine("Storage", "Data", dbName).EnsureDirectory();
            return fileName;
        }

        public static void InitializeLiteDb(string dbName = "Local_LiteDB.db")
        {
            var fileName = GetDbName();
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