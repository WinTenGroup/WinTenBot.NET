using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WinTenBot.Common;
using WinTenBot.Model;
using WinTenBot.Providers;
using WinTenBot.Services;

namespace WinTenBot.Telegram
{
    public static class WarnUsernameUtil
    {
        public static bool IsNoUsername(this User user)
        {
            var userId = user.Id;
            var ignored = new[]
            {
                "777000"
            };

            var match = ignored.FirstOrDefault(id => id == userId.ToString());
            if (!match.IsNotNullOrEmpty()) return user.Username == null;

            Log.Information("This user true Ignored!");
            return false;
        }

        public static async Task CheckUsernameAsync(this TelegramService telegramService)
        {
            try
            {
                Log.Information("Starting check Username");

                var warnLimit = 4;
                var message = telegramService.MessageOrEdited;
                var fromUser = message.From;
                var nameLink = fromUser.GetNameLink();

                // var settingService = new SettingsService(message);
                var chatSettings = telegramService.CurrentSetting;
                if (!chatSettings.EnableWarnUsername)
                {
                    Log.Information("Warn Username is disabled in this Group!");
                    return;
                }

                var noUsername = fromUser.IsNoUsername();
                Log.Information($"{fromUser} IsNoUsername: {noUsername}");

                if (noUsername)
                {
                    var updateResult = await UpdateWarnUsernameStat(message)
                        .ConfigureAwait(false);
                    var updatedStep = updateResult.StepCount;
                    var lastMessageId = updateResult.LastWarnMessageId;

                    await telegramService.DeleteAsync(lastMessageId)
                        .ConfigureAwait(false);
                    var addMinutes = Time.GetMuteStep(updatedStep);
                    var muteTime = DateTime.Now.AddMinutes(addMinutes);
                    await telegramService.RestrictMemberAsync(fromUser.Id, until: muteTime)
                        .ConfigureAwait(false);

                    var sendText = $"Hai {nameLink}, Anda belum memasang username!" +
                                   $"\nAnda telah di mute selama {addMinutes} menit (sampai dengan {muteTime}), " +
                                   $"silakan segera pasang Username lalu tekan Verifikasi agar tidak di senyapkan." +
                                   $"\nPeringatan ke {updatedStep} dari {warnLimit}";

                    if (updatedStep == warnLimit) sendText += "\n\n<b>Ini peringatan terakhir!</b>";

                    if (updatedStep > warnLimit)
                    {
                        var sendWarn = $"Batas peringatan telah di lampaui." +
                                       $"\n{nameLink} di tendang sekarang!";
                        await telegramService.SendTextAsync(sendWarn)
                            .ConfigureAwait(false);

                        await telegramService.KickMemberAsync(fromUser)
                            .ConfigureAwait(false);
                        await telegramService.UnbanMemberAsync(fromUser)
                            .ConfigureAwait(false);
                        await ResetWarnUsernameStatAsync(message)
                            .ConfigureAwait(false);

                        return;
                    }

                    var urlStart = await telegramService.GetUrlStart("start=set-username")
                        .ConfigureAwait(false);
                    Log.Information($"UrlStart: {urlStart}");

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithUrl("Cara Pasang Username", urlStart),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Verifikasi Username", "verify username")
                        }
                    });

                    var keyboard = new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithUrl("Cara Pasang Username", urlStart)
                    );

                    await telegramService.SendTextAsync(sendText, inlineKeyboard)
                        .ConfigureAwait(false);
                    await message.UpdateLastWarnUsernameMessageIdAsync(telegramService.SentMessageId)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error check Username");
            }
        }

        private static async Task<WarnUsernameHistory> UpdateWarnUsernameStat(Message message)
        {
            var tableName = "warn_username_history";

            var data = new Dictionary<string, object>
            {
                {"from_id", message.From.Id},
                {"first_name", message.From.FirstName},
                {"last_name", message.From.LastName},
                {"step_count", 1},
                {"chat_id", message.Chat.Id},
                {"created_at", DateTime.UtcNow}
            };

            var warnHistory = await new Query(tableName)
                .Where("from_id", data["from_id"])
                .Where("chat_id", data["chat_id"])
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);

            var exist = warnHistory.Any<object>();

            Log.Information($"Check Warn Username History: {exist}");

            if (exist)
            {
                var warnHistories = warnHistory.ToJson().MapObject<List<WarnUsernameHistory>>().First();

                Log.Information($"Mapped: {warnHistories.ToJson(true)}");

                var newStep = warnHistories.StepCount + 1;
                Log.Information($"New step for {message.From} is {newStep}");

                var update = new Dictionary<string, object>
                {
                    {"step_count", newStep}, {"updated_at", DateTime.UtcNow}
                };

                var insertHit = await new Query(tableName)
                    .Where("from_id", data["from_id"])
                    .Where("chat_id", data["chat_id"])
                    .ExecForMysql(true)
                    .UpdateAsync(update)
                    .ConfigureAwait(false);

                Log.Information($"Update step: {insertHit}");
            }
            else
            {
                var insertHit = await new Query(tableName)
                    .ExecForMysql(true)
                    .InsertAsync(data)
                    .ConfigureAwait(false);

                Log.Information($"Insert Hit: {insertHit}");
            }

            var updatedHistory = await new Query(tableName)
                .Where("from_id", data["from_id"])
                .Where("chat_id", data["chat_id"])
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);

            return updatedHistory.ToJson().MapObject<List<WarnUsernameHistory>>().First();
        }

        public static async Task ResetWarnUsernameStatAsync(Message message)
        {
            Log.Information("Resetting warn Username step.");

            var tableName = "warn_username_history";
            var fromId = message.From.Id;
            var chatId = message.Chat.Id;

            var update = new Dictionary<string, object>
            {
                {"step_count", 0},
                {"updated_at", DateTime.UtcNow}
            };

            var insertHit = await new Query(tableName)
                .Where("from_id", fromId)
                .Where("chat_id", chatId)
                .ExecForMysql(true)
                .UpdateAsync(update)
                .ConfigureAwait(false);

            Log.Information($"Update step: {insertHit}");
        }

        public static async Task UpdateLastWarnUsernameMessageIdAsync(this Message message, long messageId)
        {
            Log.Information("Updating last Warn MessageId.");

            var tableName = "warn_username_history";
            var fromId = message.From.Id;
            var chatId = message.Chat.Id;

            var update = new Dictionary<string, object>
            {
                {"last_warn_message_id", messageId},
                {"updated_at", DateTime.UtcNow}
            };

            var insertHit = await new Query(tableName)
                .Where("from_id", fromId)
                .Where("chat_id", chatId)
                .ExecForMysql(true)
                .UpdateAsync(update)
                .ConfigureAwait(false);

            Log.Information($"Update lastWarn: {insertHit}");
        }
    }
}