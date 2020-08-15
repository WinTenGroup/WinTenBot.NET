﻿using System;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using WinTenBot.Providers;
using WinTenBot.Services;

namespace WinTenBot.Telegram
{
    public static class SpamCheckUtil
    {
        public static async Task<bool> CheckGlobalBanAsync(this TelegramService telegramService,
            User userTarget = null)
        {
            Log.Information("Starting check Global Ban");

            var message = telegramService.MessageOrEdited;
            var user = message.From;

            // var settingService = new SettingsService(message);
            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedEs2)
            {
                Log.Information("Fed ES2 Ban is disabled in this Group!");
                return false;
            }

            if (userTarget != null) user = userTarget;

            var messageId = message.MessageId;

            var isBan = await user.Id.CheckGBan()
                .ConfigureAwait(false);
            Log.Information($"IsBan: {isBan}");
            if (isBan)
            {
                await telegramService.DeleteAsync(messageId)
                    .ConfigureAwait(false);
                await telegramService.KickMemberAsync(user)
                    .ConfigureAwait(false);
                await telegramService.UnbanMemberAsync(user)
                    .ConfigureAwait(false);
            }

            return isBan;
        }

        public static async Task<bool> CheckCasBanAsync(this TelegramService telegramService)
        {
            bool isBan;
            Log.Information("Starting check in Cas Ban");
            var message = telegramService.MessageOrEdited;
            var user = message.From;

            // var settingService = new SettingsService(message);
            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedCasBan)
            {
                Log.Information("Fed Cas Ban is disabled in this Group!");
                return false;
            }

            isBan = await user.IsCasBanAsync()
                .ConfigureAwait(false);
            Log.Information($"{user} is CAS ban: {isBan}");
            if (isBan)
            {
                var sendText = $"{user} is banned in CAS!";
                await telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
                await telegramService.KickMemberAsync(user)
                    .ConfigureAwait(false);
                await telegramService.UnbanMemberAsync(user)
                    .ConfigureAwait(false);
            }

            return isBan;
        }

        public static async Task<bool> CheckSpamWatchAsync(this TelegramService telegramService)
        {
            bool isBan;
            Log.Information("Starting Run SpamWatch");

            var message = telegramService.MessageOrEdited;
            // var settingService = new SettingsService(message);
            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFedSpamWatch)
            {
                Log.Information("Fed SpamWatch is disabled in this Group!");
                return false;
            }

            var user = message.From;
            var spamWatch = await user.Id.CheckSpamWatch()
                .ConfigureAwait(false);
            isBan = spamWatch.IsBan;

            Log.Information("{0} is SpamWatch Ban => {1}", user, isBan);

            if (isBan)
            {
                var sendText = $"{user} is banned in SpamWatch!" +
                               "\nFed: @SpamWatch" +
                               $"\nReason: {spamWatch.Reason}";
                await telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
                await telegramService.KickMemberAsync(user)
                    .ConfigureAwait(false);
                await telegramService.UnbanMemberAsync(user)
                    .ConfigureAwait(false);
            }

            return isBan;
        }
    }
}