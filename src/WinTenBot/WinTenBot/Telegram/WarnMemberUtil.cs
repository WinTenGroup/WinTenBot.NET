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
    public static class WarnMemberUtil
    {
        public static async Task WarnMemberAsync(this TelegramService telegramService)
        {
            try
            {
                Log.Information("Prepare Warning Member..");
                var message = telegramService.Message;
                var repMessage = message.ReplyToMessage;
                var textMsg = message.Text;
                var fromId = message.From.Id;
                var partText = textMsg.Split(" ");
                var reasonWarn = partText.ValueOfIndex(1) ?? "no-reason";
                var user = repMessage.From;
                Log.Information($"Warning User: {user}");

                var warnLimit = 4;
                var warnHistory = await UpdateWarnMemberStat(message)
                    .ConfigureAwait(false);
                var updatedStep = warnHistory.StepCount;
                var lastMessageId = warnHistory.LastWarnMessageId;
                var nameLink = user.GetNameLink();

                var sendText = $"{nameLink} di beri peringatan!." +
                               $"\nPeringatan ke {updatedStep} dari {warnLimit}";

                if (updatedStep == warnLimit) sendText += "\nIni peringatan terakhir!";

                if (!reasonWarn.IsNullOrEmpty())
                {
                    sendText += $"\n<b>Reason:</b> {reasonWarn}";
                }

                var muteUntil = DateTime.UtcNow.AddMinutes(3);
                await telegramService.RestrictMemberAsync(fromId, until: muteUntil)
                    .ConfigureAwait(false);

                if (updatedStep > warnLimit)
                {
                    var sendWarn = $"Batas peringatan telah di lampaui." +
                                   $"\n{nameLink} di tendang sekarang!";
                    await telegramService.SendTextAsync(sendWarn)
                        .ConfigureAwait(false);

                    await telegramService.KickMemberAsync(user)
                        .ConfigureAwait(false);
                    await telegramService.UnbanMemberAsync(user)
                        .ConfigureAwait(false);
                    await ResetWarnMemberStatAsync(message)
                        .ConfigureAwait(false);

                    return;
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Hapus peringatan", $"action remove-warn {user.Id}"),
                    }
                });

                await telegramService.SendTextAsync(sendText, inlineKeyboard)
                    .ConfigureAwait(false);
                await message.UpdateLastWarnMemberMessageIdAsync(telegramService.SentMessageId)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Warn Member");
            }
        }

        private static async Task<WarnMemberHistory> UpdateWarnMemberStat(Message message)
        {
            var tableName = "warn_member_history";
            var repMessage = message.ReplyToMessage;
            var textMsg = message.Text;
            var partText = textMsg.Split(" ");
            var reasonWarn = partText.ValueOfIndex(1) ?? "no-reason";

            var chatId = repMessage.Chat.Id;
            var fromId = repMessage.From.Id;
            var fromFName = repMessage.From.FirstName;
            var fromLName = repMessage.From.LastName;
            var warnerId = message.From.Id;
            var warnerFName = message.From.FirstName;
            var warnerLName = message.From.LastName;

            var warnHistory = await new Query(tableName)
                .Where("from_id", fromId)
                .Where("chat_id", chatId)
                .ExecForSqLite(true)
                .GetAsync()
                .ConfigureAwait(false);

            var exist = warnHistory.Any<object>();

            Log.Information($"Check Warn Username History: {exist}");

            if (exist)
            {
                var warnHistories = warnHistory.ToJson().MapObject<List<WarnMemberHistory>>().First();

                Log.Information($"Mapped: {warnHistories.ToJson(true)}");

                var newStep = warnHistories.StepCount + 1;
                Log.Information($"New step for {message.From} is {newStep}");

                var update = new Dictionary<string, object>
                {
                    {"first_name", fromFName},
                    {"last_name", fromLName},
                    {"step_count", newStep},
                    {"reason_warn", reasonWarn},
                    {"warner_first_name", warnerFName},
                    {"warner_last_name", warnerLName},
                    {"updated_at", DateTime.UtcNow}
                };

                var insertHit = await new Query(tableName)
                    .Where("from_id", fromId)
                    .Where("chat_id", chatId)
                    .ExecForSqLite(true)
                    .UpdateAsync(update)
                    .ConfigureAwait(false);

                Log.Information($"Update step: {insertHit}");
            }
            else
            {
                var data = new Dictionary<string, object>
                {
                    {"from_id", fromId},
                    {"first_name", fromFName},
                    {"last_name", fromLName},
                    {"step_count", 1},
                    {"reason_warn", reasonWarn},
                    {"warner_user_id", warnerId},
                    {"warner_first_name", warnerFName},
                    {"warner_last_name", warnerLName},
                    {"chat_id", message.Chat.Id},
                    {"created_at", DateTime.UtcNow}
                };

                var insertHit = await new Query(tableName)
                    .ExecForSqLite(true)
                    .InsertAsync(data)
                    .ConfigureAwait(false);

                Log.Information($"Insert Hit: {insertHit}");
            }

            var updatedHistory = await new Query(tableName)
                .Where("from_id", fromId)
                .Where("chat_id", chatId)
                .ExecForSqLite(true)
                .GetAsync()
                .ConfigureAwait(false);

            return updatedHistory.ToJson().MapObject<List<WarnMemberHistory>>().First();
        }

        public static async Task UpdateLastWarnMemberMessageIdAsync(this Message message, long messageId)
        {
            Log.Information("Updating last Warn Member MessageId.");

            var tableName = "warn_member_history";
            var fromId = message.ReplyToMessage.From.Id;
            var chatId = message.Chat.Id;

            var update = new Dictionary<string, object>
            {
                {"last_warn_message_id", messageId},
                {"updated_at", DateTime.UtcNow}
            };

            var insertHit = await new Query(tableName)
                .Where("from_id", fromId)
                .Where("chat_id", chatId)
                .ExecForSqLite(true)
                .UpdateAsync(update)
                .ConfigureAwait(false);

            Log.Information($"Update lastWarn: {insertHit}");
        }

        public static async Task ResetWarnMemberStatAsync(Message message)
        {
            Log.Information("Resetting warn Username step.");

            var tableName = "warn_member_history";
            var fromId = message.ReplyToMessage.From.Id;
            var chatId = message.Chat.Id;

            var update = new Dictionary<string, object>
            {
                {"step_count", 0},
                {"updated_at", DateTime.UtcNow}
            };

            var insertHit = await new Query(tableName)
                .Where("from_id", fromId)
                .Where("chat_id", chatId)
                .ExecForSqLite(true)
                .UpdateAsync(update)
                .ConfigureAwait(false);

            Log.Information($"Update step: {insertHit}");
        }

        public static async Task RemoveWarnMemberStatAsync(this TelegramService telegramService, int userId)
        {
            Log.Information("Removing warn Member stat.");

            var tableName = "warn_member_history";
            var message = telegramService.Message;
            var chatId = message.Chat.Id;

            var update = new Dictionary<string, object>
            {
                {"step_count", 0},
                {"updated_at", DateTime.UtcNow}
            };

            var insertHit = await new Query(tableName)
                .Where("from_id", userId)
                .Where("chat_id", chatId)
                .ExecForSqLite(true)
                .UpdateAsync(update)
                .ConfigureAwait(false);

            Log.Information($"Update step: {insertHit}");
        }
    }
}