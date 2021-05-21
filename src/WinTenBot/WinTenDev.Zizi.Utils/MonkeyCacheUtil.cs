using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonkeyCache;
using MonkeyCache.FileStore;
using Serilog;
using Telegram.Bot.Types;
using WinTenDev.Zizi.Utils.IO;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Utils
{
    [Obsolete("MonkeyCache will be replaced with EasyCaching")]
    public static class MonkeyCacheUtil
    {
        public static void SetupCache()
        {
            Log.Information("Initializing MonkeyCache");
            var cachePath = Path.Combine("Storage", "MonkeyCache").SanitizeSlash().EnsureDirectory(true);
            Barrel.ApplicationId = "ZiziBot-Cache";
            BarrelUtils.SetBaseCachePath(cachePath);

            Log.Debug("Deleting old MonkeyCache");
            DeleteKeys();

            Log.Information("MonkeyCache is ready.");
        }

        public static bool IsCacheExist(string key)
        {
            var isExist = Barrel.Current.Exists(key);
            var isExpired = Barrel.Current.IsExpired(key);
            var expired = Barrel.Current.GetExpiration(key)?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss tt zz");

            var isValid = isExist && !isExpired;

            Log.Debug("MonkeyCache key {0} isExist {1} isExpired {2} until {3}", key, isExist, isExpired, expired);

            return isValid;
        }

        public static void AddCache<T>(this T data, string key, int expireIn = 1)
        {
            var expireDate = TimeSpan.FromMinutes(expireIn);
            Log.Debug("Adding Monkeys with key: '{0}'. Expire in: '{1}' ", key, expireDate);
            Barrel.Current.Add(key, data, expireDate);
        }

        public static T Get<T>(string key)
        {
            Log.Information("Getting Monkeys data: {0}", key);
            var data = Barrel.Current.Get<T>(key);
            // Log.Debug($"Monkey Data of '{key}': {data.ToJson(true)}");

            return data;
        }

        public static IEnumerable<string> GetKeys()
        {
            Log.Information("Getting all Monkeys Cache");
            var keys = Barrel.Current.GetKeys();
            Log.Debug("MonKeys: {0}", keys.ToJson(true));

            return keys;
        }

        public static void DeleteExpired()
        {
            Log.Information("Deleting Expired MonkeyCache");
            Barrel.Current.EmptyExpired();
            Log.Debug("Delete done.");
        }

        public static void DeleteKeys(string prefix = "")
        {
            var keys = GetKeys().Where(s => s.Contains(prefix));

            if (prefix.IsNotNullOrEmpty()) keys = GetKeys();
            Log.Debug("Deleting MonkeyCache following keys: {0}", keys.ToJson(true));

            Barrel.Current.Empty(keys.ToArray());
            Log.Debug("Delete done.");
        }

        public static T SetChatCache<T>(this Message msg, string key, T data)
        {
            var chatId = msg.Chat.Id.ReduceChatId();
            var msgId = msg.MessageId;
            var keyCache = $"{chatId}-{key}";

            AddCache(data, keyCache);
            return data;
        }

        public static T GetChatCache<T>(this Message msg, string key)
        {
            var chatId = msg.Chat.Id.ReduceChatId();
            var msgId = msg.MessageId;
            var keyCache = $"{chatId}-{key}";

            var data = Get<T>(keyCache);
            return data;
        }
    }
}