﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services;
using String = System.String;

namespace Zizi.Bot.Telegram
{
    public static class Chats
    {
        public static long ReduceChatId(this long chatId)
        {
            var chatIdStr = chatId.ToString();
            if (chatIdStr.StartsWith("-100"))
            {
                chatIdStr = chatIdStr.Substring(4);
            }

            $"Reduced ChatId from {chatId} to {chatIdStr}".LogDebug();

            return chatIdStr.ToInt64();
        }

        public static async Task EnsureChatHealthAsync(this TelegramService telegramService)
        {
            Log.Debug("Ensuring chat health..");

            var message = telegramService.Message;
            var chatId = message.Chat.Id;
            var settingsService = new SettingsService
            {
                Message = message
            };

            var isExist = await settingsService.IsSettingExist().ConfigureAwait(false);
            if (isExist)
            {
                Log.Debug("Settings for chatId {0} is exist, done.", chatId);
                return;
            }

            Log.Information("preparing restore health on chatId {0}..", chatId);
            var data = new Dictionary<string, object>()
            {
                {"chat_id", chatId},
                {"chat_title", message.Chat.Title ?? @"N\A"},
                {"chat_type", message.Chat.Type.ToString()}
            };

            var saveSettings = await settingsService.SaveSettingsAsync(data)
                .ConfigureAwait(false);
            Log.Debug($"Ensure Settings: {saveSettings}");

            await settingsService.UpdateCache()
                .ConfigureAwait(false);
        }

        #region Chat Restriction

        public static bool IsRestricted()
        {
            var isRestricted = BotSettings.GlobalConfiguration["CommonConfig:IsRestricted"].ToBool();
            Log.Debug("Global Restriction: {0}", isRestricted);

            return isRestricted;
        }

        public static bool CheckRestriction(this long chatId)
        {
            try
            {
                var isRestricted = false;
                var sudoers = BotSettings.GlobalConfiguration.GetSection("RestrictArea").Get<List<string>>();
                var match = sudoers.FirstOrDefault(x => x == chatId.ToString());

                if (match == null)
                {
                    isRestricted = true;
                }

                Log.Information("ChatId: {0} IsRestricted: {1}", chatId, isRestricted);
                return isRestricted;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Chat Restriction.");
                return false;
            }
        }

        public static async Task<bool> EnsureChatRestrictionAsync(this TelegramService telegramService)
        {
            Log.Information("Starting ensure Chat Restriction");

            var message = telegramService.MessageOrEdited;
            var chatId = message.Chat.Id;

            var globalRestrict = IsRestricted();
            var isRestricted = chatId.CheckRestriction();

            if (!isRestricted && globalRestrict) return false;

            Log.Information("I must leave right now!");
            var msgOut = $"Sepertinya saya salah alamat, saya pamit dulu..";

            await telegramService.SendTextAsync(msgOut)
                .ConfigureAwait(false);
            await telegramService.LeaveChat(chatId)
                .ConfigureAwait(false);
            return true;
        }

        #endregion
    }
}