using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JsonFlatFileDataStore;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Telegram
{
    public static class WarnUsernameUtil
    {
        private const string JsonName = "warn-username";
        private const string tableName = "warn_username_history";

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
            var sw = Stopwatch.StartNew();
            // var warnJson = JsonName.OpenJson();

            try
            {
                Log.Information("Starting check Username");

                // var warnUsername = await warnJson.GetCollectionAsync<WarnUsernameHistory>()
                // .ConfigureAwait(false);

                var warnLimit = 4;
                // var errorCount = 0;
                var message = telegramService.MessageOrEdited;
                var fromUser = message.From;
                var fromId = fromUser.Id;
                var chatId = message.Chat.Id;
                var nameLink = fromUser.GetNameLink();

                // var settingService = new SettingsService(message);
                var chatSettings = telegramService.CurrentSetting;
                var lastWarn = chatSettings.LastWarnUsernameMessageId;

                if (!chatSettings.EnableWarnUsername)
                {
                    Log.Information("Warn Username is disabled in this Group!");
                    return;
                }

                var isBotAdmin = await telegramService.IsBotAdmin().ConfigureAwait(false);
                if (!isBotAdmin)
                {
                    Log.Information("This bot IsNot Admin in {0}, so Warn Username disabled.", message.Chat);
                    return;
                }

                var noUsername = fromUser.IsNoUsername();

                Log.Information("{0} IsNoUsername: {1}", fromUser, noUsername);

                if (noUsername)
                {
                    var isFromAdmin = await telegramService.IsAdminGroup().ConfigureAwait(false);
                    if (isFromAdmin)
                    {
                        Log.Information("This UserID {0} Is Admin in {1}, so Warn Username disabled.", fromId, message.Chat.Id);
                        return;
                    }

                    var settingsService = new SettingsService(message);
                    var updateResult = (await UpdateWarnUsernameStat(message)
                            .ConfigureAwait(false))
                        .ToList();
                    var noUsernameCount = updateResult.Count;

                    Log.Debug("No Username Count: {0}", noUsernameCount);

                    // var listNoUsername = updateResult
                    //     .Select(x =>
                    //     {
                    //         var id = x.FromId;
                    //         var step = x.StepCount;
                    //         var nameL = id.GetNameLink(x.FirstName + $" (Step {step})");
                    //         return nameL;
                    //     });

                    // var nameLinks = string.Join("\n", listNoUsername);
                    // var sendText =
                    //     $"Terdapat {noUsernameCount} Anggota yang belum memasang Username" +
                    //     $"\n\n{nameLinks}\n" +
                    //     "\nMasing-masing telah di mute berdasarkan <a href='https://t.me/WinTenDev/41'>Step Count</a> " +
                    //     $"silakan segera pasang Username lalu tekan Verifikasi agar tidak di senyapkan.";

                    // $"\nPeringatan ke {updatedStep} dari {warnLimit}";
                    // $"\nAnda telah di mute selama {addMinutes} menit (sampai dengan {muteTime}), " +

                    // if (updatedStep == warnLimit) sendText += "\n\n<b>Ini peringatan terakhir!</b>";

                    // var orderDescFirst = updateResult
                    //     .OrderByDescending(x => x.LastWarnMessageId).First();
                    var currentUser = updateResult.First(x => x.FromId == fromId);

                    // foreach (var usernameHistory in updateResult)
                    // {
                    var updatedStep = currentUser.StepCount;
                    // var lastMessageId = orderDescFirst.LastWarnMessageId;

                    await telegramService.DeleteAsync(lastWarn.ToInt())
                        .ConfigureAwait(false);
                    var addMinutes = updatedStep.GetMuteStep();
                    var muteTime = DateTime.Now.AddMinutes(addMinutes);
                    var result = await telegramService.RestrictMemberAsync(fromUser.Id, until: muteTime)
                        .ConfigureAwait(false);

                    if (!result.IsSuccess)
                    {
                        // errorCount++;

                        var exception = result.Exception;
                        var exMessage = exception.Message;

                        var exSend =
                            "Sepertinya ZiZi gagal membisukan Anggota, " +
                            "pastikan ZiZi menjadi Admin dengan " +
                            "<a href='https://docs.zizibot.azhe.space/glosarium/admin-dengan-level-standard'>Level Standard</a>";

                        await telegramService.SendTextAsync(exSend)
                            .ConfigureAwait(false);

                        // break;
                        return;
                    }

                    if (updatedStep > warnLimit)
                    {
                        var sendWarn = $"Batas peringatan telah di lampaui." +
                                       $"\n{nameLink} di tendang sekarang!";

                        await telegramService.SendTextAsync(sendWarn, disableWebPreview: true)
                            .ConfigureAwait(false);

                        await telegramService.KickMemberAsync(fromUser)
                            .ConfigureAwait(false);
                        await telegramService.UnbanMemberAsync(fromUser)
                            .ConfigureAwait(false);
                        await ResetWarnUsernameStatAsync(fromId, chatId)
                            .ConfigureAwait(false);

                        // return;
                    }
                    // }

                    //
                    // var urlStart = await telegramService.GetUrlStart("start=set-username")
                    //     .ConfigureAwait(false);
                    // Log.Information($"UrlStart: {urlStart}");
                    // var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    // {
                    //     new[]
                    //     {
                    //         InlineKeyboardButton.WithUrl("Cara Pasang Username", urlStart),
                    //     },
                    //     new[]
                    //     {
                    //         InlineKeyboardButton.WithCallbackData("Verifikasi Username", "verify username")
                    //
                    //
                    // }
                    // });

                    // var keyboard = new InlineKeyboardMarkup(
                    //     InlineKeyboardButton.WithUrl("Cara Pasang Username", urlStart)
                    // );

                    // await telegramService.SendTextAsync(sendText, inlineKeyboard, disableWebPreview: true)
                    // .ConfigureAwait(false);
                    // await message.UpdateLastWarnUsernameMessageIdAsync(telegramService.SentMessageId)
                    //     .ConfigureAwait(false);

                    await telegramService.UpdateWarnMessageAsync()
                        .ConfigureAwait(false);

                    await settingsService.UpdateCell("last_warn_username_message_id", telegramService.SentMessageId)
                        .ConfigureAwait(false);
                }
                else
                {
                    Log.Information("UserId: {0} have Username", fromId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error check Username");
            }
            // finally
            // {
            // warnJson.Dispose();
            // }

            Log.Information("Check Username finish. {0}", sw.Elapsed);
        }

        public static async Task<IDocumentCollection<WarnUsernameHistory>> GetWarnUsernameCollectionAsync()
        {
            var warnJson = JsonName.OpenJson();
            var warnHistories = (await warnJson.GetCollectionAsync<WarnUsernameHistory>()
                .ConfigureAwait(false));

            return warnHistories;
        }

        public static async Task<IEnumerable<WarnUsernameHistory>> GetWarnUsernameHistory(Message message)
        {
            var warnJson = JsonName.OpenJson();
            var fromId = message.From.Id;
            var firstName = message.From.FirstName;
            var lastName = message.From.LastName;
            var chatId = message.Chat.Id;

            var warnHistories = (await warnJson.GetCollectionAsync<WarnUsernameHistory>()
                    .ConfigureAwait(false))
                .AsQueryable()
                .Where(x => x.ChatId == chatId);

            warnJson.Dispose();

            return warnHistories;
        }

        public static async Task UpdateWarnMessageAsync(this TelegramService telegramService)
        {
            var message = telegramService.Message;
            var callback = telegramService.CallbackQuery;

            var updateResult = (await GetWarnUsernameHistory(message)
                    .ConfigureAwait(false))
                .ToList()
                .OrderBy(x => x.StepCount)
                .ToList();

            var noUsernameCount = updateResult.Count;
            var listLimit = 5;

            Log.Debug("No Username Count: {0}", noUsernameCount);

            if (noUsernameCount == 0)
            {
                Log.Debug("Seem all Members have set Username");

                var callbackMsgId = callback.Message.MessageId;

                await telegramService.DeleteAsync(callbackMsgId).ConfigureAwait(false);
            }
            else
            {
                var listNoUsername = updateResult
                    .Select(x =>
                    {
                        var id = x.FromId;
                        var step = x.StepCount;
                        var nameL = id.GetNameLink(x.FirstName + $" (Step mute {step})");
                        return nameL;
                    }).Take(listLimit);

                var diff = 0;

                if (noUsernameCount > listLimit)
                {
                    diff = noUsernameCount - listLimit;
                }

                // var mentionAll = CreateMentionAll(updateResult);

                // var nameLinks = string.Join("\n", listNoUsername);
                var sendText = $"Terdapat {noUsernameCount} Anggota yang belum memasang Username.\n";
                // $"\n\n{nameLinks}\n";

                // if (diff > 0)
                // {
                //     sendText += $"dan {diff} lainnya.{mentionAll}\n";
                // }

                sendText += "\nMasing-masing telah di mute berdasarkan <a href='https://t.me/WinTenDev/41'>Step Mute</a> " +
                            $"silakan segera pasang Username dan jangan lupa tekan tombol Verifikasi di bawah ini.";

                var urlStart = await telegramService.GetUrlStart("start=set-username")
                    .ConfigureAwait(false);
                Log.Information("UrlStart: {UrlStart}", urlStart);

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

                // var keyboard = new InlineKeyboardMarkup(
                //     InlineKeyboardButton.WithUrl("Cara Pasang Username", urlStart)
                // );

                if (callback != null)
                {
                    var callbackMsgId = callback.Message.MessageId;

                    Log.Debug("Me should edit current Message");

                    await telegramService.EditMessageCallback(sendText, inlineKeyboard, disableWebPreview: true)
                        .ConfigureAwait(false);
                }
                else
                {
                    await telegramService.SendTextAsync(sendText, inlineKeyboard, disableWebPreview: true, replyToMsgId: 0)
                        .ConfigureAwait(false);
                }
            }
        }

        private static async Task<IEnumerable<WarnUsernameHistory>> UpdateWarnUsernameStat(Message message)
        {
            var warnJson = JsonName.OpenJson();
            var warnUsername = await GetWarnUsernameCollectionAsync();
            var fromId = message.From.Id;
            var firstName = message.From.FirstName;
            var lastName = message.From.LastName;
            var chatId = message.Chat.Id;

            // var data = new Dictionary<string, object>
            // {
            //     {"from_id", message.From.Id},
            //     {"first_name", message.From.FirstName},
            //     {"last_name", message.From.LastName},
            //     {"step_count", 1},
            //     {"chat_id", message.Chat.Id},
            //     {"created_at", DateTime.UtcNow}
            // };

            // var warnHistories = (await warnJson.GetCollectionAsync<WarnUsernameHistory>()
            //         .ConfigureAwait(false))
            //     .AsQueryable()
            //     .Where(x => x.FromId == fromId && x.ChatId == chatId);

            var warnHistories = warnUsername.AsQueryable()
                .Where(x => x.ChatId == chatId && x.FromId == fromId);

            // var warnHistory = await new Query(tableName)
            //     .Where("from_id", data["from_id"])
            //     .Where("chat_id", data["chat_id"])
            //     .ExecForMysql(true)
            //     .GetAsync()
            //     .ConfigureAwait(false);

            // var exist = warnHistory.Any<object>();
            var exist = warnHistories.Any();

            Log.Information("Check Warn Username History: {0}", exist);

            if (exist)
            {
                Log.Debug("Updating Step for {0}", fromId);
                // var warnHistories = warnHistory.ToJson().MapObject<List<WarnUsernameHistory>>().First();
                var warnHistory = warnHistories.First();

                Log.Information("Mapped: {0}", warnHistories.ToJson(true));

                // var newStep = warnHistories.StepCount + 1;
                var newStep = warnHistory.StepCount + 1;
                Log.Information("New step for {From} is {NewStep}", message.From, newStep);

                // var update = new Dictionary<string, object>
                // {
                //     {"step_count", newStep}, {"updated_at", DateTime.UtcNow}
                // };

                // var insertHit = await new Query(tableName)
                //     .Where("from_id", data["from_id"])
                //     .Where("chat_id", data["chat_id"])
                //     .ExecForMysql(true)
                //     .UpdateAsync(update)
                //     .ConfigureAwait(false);

                var insertHit = await warnUsername.UpdateOneAsync(x =>
                        x.FromId == fromId && x.ChatId == chatId,
                    new WarnUsernameHistory()
                    {
                        FromId = fromId,
                        FirstName = firstName,
                        LastName = lastName,
                        StepCount = newStep,
                        ChatId = chatId,
                        CreatedAt = warnHistory.CreatedAt,
                        UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }).ConfigureAwait(false);

                Log.Information("Update step: {0}", insertHit);
            }
            else
            {
                Log.Debug("Inserting Step for {0}", fromId);

                // var insertHit = await new Query(tableName)
                //     .ExecForMysql(true)
                //     .InsertAsync(data)
                //     .ConfigureAwait(false);

                var insertHit = await warnUsername.InsertOneAsync(new WarnUsernameHistory()
                {
                    FromId = fromId,
                    FirstName = firstName,
                    LastName = lastName,
                    StepCount = 1,
                    ChatId = chatId,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }).ConfigureAwait(false);

                Log.Information("Insert Hit: {0}", insertHit);
            }

            // var updatedHistory = await new Query(tableName)
            //     .Where("from_id", data["from_id"])
            //     .Where("chat_id", data["chat_id"])
            //     .ExecForMysql(true)
            //     .GetAsync()
            //     .ConfigureAwait(false);

            // return updatedHistory.ToJson().MapObject<List<WarnUsernameHistory>>().First();

            warnJson.Dispose();

            // var updatedHistory = (await warnJson.GetCollectionAsync<WarnUsernameHistory>()
            //         .ConfigureAwait(false))
            //     .AsQueryable()
            //     .Where(x => x.FromId == fromId && x.ChatId == chatId);

            return (await GetWarnUsernameCollectionAsync().ConfigureAwait(false))
                .AsQueryable()
                .Where(x => x.ChatId == chatId);
        }

        public static async Task ResetWarnUsernameStatAsync(int fromId, long chatId)
        {
            Log.Information("Resetting warn Username step.");

            var warnJson = JsonName.OpenJson();
            var warnUsername = await warnJson.GetCollectionAsync<WarnUsernameHistory>()
                .ConfigureAwait(false);

            // var tableName = "warn_username_history";
            // var fromId = message.From.Id;
            // var chatId = message.Chat.Id;

            // var update = new Dictionary<string, object>
            // {
            //     {"step_count", 0},
            //     {"updated_at", DateTime.UtcNow}
            // };

            //
            // var resetWarn = await new Query(tableName)
            //     .Where("from_id", fromId)
            //     .Where("chat_id", chatId)
            //     .ExecForMysql(true)
            //     .UpdateAsync(update)
            //     .ConfigureAwait(false);

            var resetWarn = await warnUsername.DeleteOneAsync(x =>
                    x.FromId == fromId && x.ChatId == chatId)
                .ConfigureAwait(false);

            warnJson.Dispose();

            Log.Information("Update step: {0}", resetWarn);
        }

        public static async Task UpdateLastWarnUsernameMessageIdAsync(this Message message, long messageId)
        {
            var sw = Stopwatch.StartNew();
            Log.Information("Updating last Warn MessageId.");

            // var tableName = "warn_username_history";
            var warnUsername = await GetWarnUsernameCollectionAsync();

            var fromId = message.From.Id;
            var chatId = message.Chat.Id;

            var current = warnUsername
                .AsQueryable().First(x => x.FromId == fromId && x.ChatId == chatId);

            current.LastWarnMessageId = messageId.ToInt();
            current.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var insertHit = await warnUsername.UpdateOneAsync(x =>
                x.FromId == fromId && x.ChatId == chatId, current).ConfigureAwait(false);

            // var update = new Dictionary<string, object>
            // {
            //     {"last_warn_message_id", messageId},
            //     {"updated_at", DateTime.UtcNow}
            // };
            //
            // var insertHit = await new Query(tableName)
            //     .Where("from_id", fromId)
            //     .Where("chat_id", chatId)
            //     .ExecForMysql(true)
            //     .UpdateAsync(update)
            //     .ConfigureAwait(false);

            Log.Information("Update lastWarn: {0}. In {1}", insertHit, sw.Elapsed);
        }

        private static string CreateMentionAll(IEnumerable<WarnUsernameHistory> warnUsernameHistories)
        {
            var mentionAll = string.Empty;
            foreach (var usernameHistory in warnUsernameHistories)
            {
                var userId = usernameHistory.FromId;
                var nameLink = userId.GetNameLink("&#8203;");

                mentionAll += $"{nameLink}";
            }

            return mentionAll;
        }
    }
}