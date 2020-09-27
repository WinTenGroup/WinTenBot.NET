using System.IO;
using System.Threading.Tasks;
using JsonFlatFileDataStore;
using Serilog;
using Zizi.Bot.IO;

namespace Zizi.Bot.Tools
{
    public static class JsonCacheUtil
    {
        private static string BasePath { get; } = Path.Combine("Storage", "JsonCache");

        public static DataStore OpenJson(this string path)
        {
            var file = Path.Combine(BasePath, path + ".json").SanitizeSlash().EnsureDirectory();
            Log.Information("Opening Json: {0}", file);
            var store = new DataStore(file);
            return store;
        }

        public static async Task<IDocumentCollection<T>> GetCollectionAsync<T>(this DataStore dataStore) where T : class
        {
            return await Task.Run(() =>
            {
                var collection = dataStore.GetCollection<T>();
                return collection;
            }).ConfigureAwait(false);

        }
    }
}