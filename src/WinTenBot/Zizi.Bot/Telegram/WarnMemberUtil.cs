using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
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
                Log.Information("Warning User: {User}", user);

                var warnLimit = 4;
                var warnHistory = await UpdateWarnMemberStat(message);
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
                await telegramService.RestrictMemberAsync(fromId, until: muteUntil);

                if (updatedStep > warnLimit)
                {
                    var sendWarn = $"Batas peringatan telah di lampaui." +
                                   $"\n{nameLink} di tendang sekarang!";
                    await telegramService.SendTextAsync(sendWarn);

                    await telegramService.KickMemberAsync(user);
                    await telegramService.UnbanMemberAsync(user);
                    await ResetWarnMemberStatAsync(message);

                    return;
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Hapus peringatan", $"action remove-warn {user.Id}"),
                    }
                });

                await telegramService.SendTextAsync(sendText, inlineKeyboard);
                await message.UpdateLastWarnMemberMessageIdAsync(telegramService.SentMessageId);
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
                .GetAsync();

            var exist = warnHistory.Any<object>();

            Log.Information("Check Warn Username History: {Exist}", exist);

            if (exist)
            {
                var warnHistories = warnHistory.ToJson().MapObject<List<WarnMemberHistory>>().First();

                Log.Information("Mapped: {V}", warnHistories.ToJson(true));

                var newStep = warnHistories.StepCount + 1;
                Log.Information("New step for {From} is {NewStep}", message.From, newStep);

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
                    .UpdateAsync(update);

                Log.Information("Update step: {InsertHit}", insertHit);
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
                    .InsertAsync(data);

                Log.Information("Insert Hit: {InsertHit}", insertHit);
            }

            var updatedHistory = await new Query(tableName)
                .Where("from_id", fromId)
                .Where("chat_id", chatId)
                .ExecForSqLite(true)
                .GetAsync();

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
                .UpdateAsync(update);

            Log.Information("Update lastWarn: {InsertHit}", insertHit);
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
                .UpdateAsync(update);

            Log.Information("Update step: {InsertHit}", insertHit);
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
                .UpdateAsync(update);

            Log.Information("Update step: {InsertHit}", insertHit);
        }
    }
}