using System;
using System.IO;
using System.Threading.Tasks;
using JsonFlatFileDataStore;
using Serilog;
using Zizi.Bot.IO;
using Zizi.Bot.Services;

namespace Zizi.Bot.Tools
{
    [Obsolete("JsonCache util will be replaced by other provider.")]
    public static class JsonCacheUtil
    {
        private static string BasePath { get; } = Path.Combine("Storage", "JsonCache");

        private static string GetJsonPath(string path)
        {
            return Path.Combine(BasePath, path + ".json").SanitizeSlash().EnsureDirectory();
        }

        public static DataStore OpenJson(this string path)
        {
            var file = Path.Combine(BasePath, path + ".json").SanitizeSlash().EnsureDirectory();
            Log.Information("Opening Json: {0}", file);
            var store = new DataStore(file);
            return store;
        }

        public static IDocumentCollection<T> GetChatCollection<T>(this TelegramService telegramService, string cachePath) where T : class
        {
            var message = telegramService.Message;
            var chatId = message.Chat.Id.ToString();
            var path = Path.Combine(chatId, cachePath);

            var jsonPath = GetJsonPath(path);
            Log.Debug("Loading JSON cache. Path: {0}", jsonPath);

            var dataStore = new DataStore(jsonPath);
            return dataStore.GetCollection<T>();
        }

        public static Task<IDocumentCollection<T>> GetChatCollectionAsync<T>(this TelegramService telegramService, string cachePath) where T : class
        {
            Log.Debug("Async JSON cache load.");
            return Task.Run(() => telegramService.GetChatCollection<T>(cachePath));
        }

        public static async Task<IDocumentCollection<T>> GetCollectionAsync<T>(this DataStore dataStore) where T : class
        {
            return await Task.Run(() =>
            {
                var collection = dataStore.GetCollection<T>();
                return collection;
            });
        }
    }
}