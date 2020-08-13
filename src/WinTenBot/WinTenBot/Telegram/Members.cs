using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using WinTenBot.Tools;

namespace WinTenBot.Telegram
{
    public static class Members
    {
        public static string GetNameLink(int userId, string name)
        {
            return $"<a href='tg://user?id={userId}'>{name}</a>";
        }

        public static string GetNameLink(this User user)
        {
            var firstName = user.FirstName;
            var lastName = user.LastName;

            return $"<a href='tg://user?id={user.Id}'>{(firstName + " " + lastName).Trim()}</a>";
        }

        public static string GetFromNameLink(this Message message)
        {
            var firstName = message.From.FirstName;
            var lastName = message.From.LastName;

            return $"<a href='tg://user?id={message.From.Id}'>{(firstName + " " + lastName).Trim()}</a>";
        }

        public static async Task AfkCheckAsync(this TelegramService telegramService)
        {
            Log.Information("Starting check AFK");

            var message = telegramService.MessageOrEdited;

            // var settingService = new SettingsService(message);
            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableAfkStat)
            {
                Log.Information("Afk Stat is disabled in this Group!");
                return;
            }

            var afkService = new AfkService();
            if (message.ReplyToMessage != null)
            {
                var repMsg = message.ReplyToMessage;
                var isAfkReply = await afkService.IsAfkAsync(repMsg)
                    .ConfigureAwait(false);
                if (isAfkReply)
                    await telegramService.SendTextAsync($"{repMsg.GetFromNameLink()} sedang afk")
                        .ConfigureAwait(false);
            }

            var isAfk = await afkService.IsAfkAsync(message)
                .ConfigureAwait(false);
            if (isAfk)
            {
                await telegramService.SendTextAsync($"{message.GetFromNameLink()} sudah tidak afk")
                    .ConfigureAwait(false);

                var data = new Dictionary<string, object>
                {
                    {"chat_id", message.Chat.Id}, {"user_id", message.From.Id}, {"is_afk", 0}, {"afk_reason", ""}
                };

                await afkService.SaveAsync(data).ConfigureAwait(false);
                await afkService.UpdateCacheAsync().ConfigureAwait(false);
            }
        }

        public static async Task CheckMataZiziAsync(this TelegramService telegramService)
        {
            try
            {
                var message = telegramService.MessageOrEdited;
                var fromId = message.From.Id;
                var fromUsername = message.From.Username;
                var fromFName = message.From.FirstName;
                var fromLName = message.From.LastName;
                var chatId = message.Chat.Id;
                var botUser = await telegramService.GetMeAsync()
                    .ConfigureAwait(false);

                Log.Information("Starting SangMata check..");

                // var query = await new Query("hit_activity")
                //     .ExecForMysql(true)
                //     .Where("from_id", fromId)
                //     .Where("chat_id", chatId)
                //     .OrderByDesc("timestamp")
                //     .Limit(1)
                //     .GetAsync()
                //     .ConfigureAwait(false);

                // if (!query.Any())
                // {
                //     Log.Information($"This may first Hit from User {fromId}");
                //     return;
                // }

                // var hitActivity = query.ToJson().MapObject<List<HitActivity>>().FirstOrDefault();
                
                var hitActivity = telegramService.GetChatCache<HitActivity>(fromId.ToString());
                if (hitActivity == null)
                {
                    Log.Information($"This may first Hit from User {0}", fromId);
                    
                    telegramService.SetChatCache(fromId.ToString(), new HitActivity()
                    {
                        ViaBot = botUser.Username,
                        MessageType = message.Type.ToString(),
                        FromId = message.From.Id,
                        FromFirstName = message.From.FirstName,
                        FromLastName = message.From.LastName,
                        FromUsername = message.From.Username,
                        FromLangCode = message.From.LanguageCode,
                        ChatId = message.Chat.Id.ToString(),
                        ChatUsername = message.Chat.Username,
                        ChatType = message.Chat.Type.ToString(),
                        ChatTitle = message.Chat.Title,
                        Timestamp = DateTime.Now
                    });
                    
                    return;
                }

                Log.Debug($"SangMata: {hitActivity.ToJson(true)}");

                var changesCount = 0;
                var msgBuild = new StringBuilder();

                msgBuild.AppendLine("😽 <b>MataZizi</b>");
                msgBuild.AppendLine($"<b>UserID:</b> {fromId}");

                if (fromUsername != hitActivity.FromUsername)
                {
                    Log.Information("Username changed detected!");
                    msgBuild.AppendLine($"Mengubah Username menjadi @{fromUsername}");
                    changesCount++;
                }

                if (fromFName != hitActivity.FromFirstName)
                {
                    Log.Information("First Name changed detected!");
                    msgBuild.AppendLine($"Mengubah nama depan menjadi {fromFName}");
                    changesCount++;
                }

                if (fromLName != hitActivity.FromLastName)
                {
                    Log.Information("Last Name changed detected!");
                    msgBuild.AppendLine($"Mengubah nama belakang menjadi {fromLName}");
                    changesCount++;
                }

                if (changesCount > 0)
                {
                    await telegramService.SendTextAsync(msgBuild.ToString().Trim())
                        .ConfigureAwait(false);

                    telegramService.SetChatCache(fromId.ToString(), new HitActivity()
                    {
                        ViaBot = botUser.Username,
                        MessageType = message.Type.ToString(),
                        FromId = message.From.Id,
                        FromFirstName = message.From.FirstName,
                        FromLastName = message.From.LastName,
                        FromUsername = message.From.Username,
                        FromLangCode = message.From.LanguageCode,
                        ChatId = message.Chat.Id.ToString(),
                        ChatUsername = message.Chat.Username,
                        ChatType = message.Chat.Type.ToString(),
                        ChatTitle = message.Chat.Title,
                        Timestamp = DateTime.Now
                    });
                    Log.Debug("Complete update Cache");
                }

                Log.Information("MataZizi completed. Changes: {0}", changesCount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error SangMata");
            }
        }

        #region AntiSpam

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

            Log.Information($"{user} is SpamWatch Ban => {isBan}");

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

        #endregion

        #region Manual Warn Member

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

            var exist = warnHistory.Any();

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

        #endregion

        #region Check Username

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
                    var addMinutes = updatedStep.GetMuteStep();
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

            var exist = warnHistory.Any();

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

        #endregion
    }
}