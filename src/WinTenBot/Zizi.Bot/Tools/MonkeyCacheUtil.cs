using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonkeyCache;
using MonkeyCache.FileStore;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Tools
{
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
            var expired = Barrel.Current.GetExpiration(key);

            var isValid = isExist || !isExpired;

            Log.Debug("MonkeyCache key {0} isExist {1} isExpired {2} until {3}", key, isExist, isExpired, expired);

            return isValid;
        }

        public static void AddCache<T>(this T data, string key)
        {
            Log.Information("Adding Monkeys with key: {0}", key);
            Barrel.Current.Add(key, data, TimeSpan.FromDays(1));
        }

        public static T Get<T>(string key)
        {
            Log.Information("Getting Monkeys data: {0}", key);
            return Barrel.Current.Get<T>(key);
        }

        public static IEnumerable<string> GetKeys()
        {
            Log.Information("Getting all Monkeys Cache");
            var keys = Barrel.Current.GetKeys();
            Log.Debug("MonKeys: {0}", keys.ToJson(true));

            return keys;
        }

        public static void DeleteKeys(string prefix = "")
        {
            var keys = GetKeys().Where(s => s.Contains(prefix));

            if (prefix.IsNotNullOrEmpty()) keys = GetKeys();
            Log.Debug("Deleting MonkeyCache following keys: {0}", keys.ToJson(true));

            Barrel.Current.Empty(keys.ToArray());
        }

        public static bool IsChatCacheExist(this TelegramService telegramService, string key)
        {
            var msg = telegramService.AnyMessage;
            var chatId = msg.Chat.Id.ReduceChatId();
            var keyCache = $"{chatId}-{key}";

            var isValid = IsCacheExist(keyCache);

            return isValid;
        }

        public static T SetChatCache<T>(this Message msg, string key, T data)
        {
            var chatId = msg.Chat.Id.ReduceChatId();
            var msgId = msg.MessageId;
            var keyCache = $"{chatId}-{key}";

            AddCache(data, keyCache);
            return data;
        }

        public static T SetChatCache<T>(this TelegramService telegramService, string key, T data)
        {
            var msg = telegramService.AnyMessage;
            msg.SetChatCache(key, data);

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

        public static T GetChatCache<T>(this TelegramService telegramService, string key)
        {
            var msg = telegramService.AnyMessage;
            var data = msg.GetChatCache<T>(key);

            return data;
        }
    }
}