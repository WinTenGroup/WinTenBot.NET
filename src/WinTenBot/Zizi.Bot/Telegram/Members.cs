using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Tools;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class Members
    {
        public static string GetNameLink(this int userId, string name)
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

        [Obsolete("AFK Check will be moved to NewUpdateHandler")]
        public static async Task AfkCheckAsync(this TelegramService telegramService)
        {
            var sw = Stopwatch.StartNew();
            Log.Information("Starting check AFK");

            var message = telegramService.MessageOrEdited;

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

            Log.Debug("AFK check completed. In {0}", sw.Elapsed);
            sw.Stop();
        }

        public static async Task CheckMataZiziAsync(this TelegramService telegramService)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var message = telegramService.MessageOrEdited;
                var fromId = message.From.Id;
                var fromUsername = message.From.Username;
                var fromFName = message.From.FirstName;
                var fromLName = message.From.LastName;
                var chatId = message.Chat.Id;

                var chatSettings = telegramService.CurrentSetting;
                if (!chatSettings.EnableZiziMata)
                {
                    Log.Information("MataZizi is disabled in this Group!. Completed in {0}", sw.Elapsed);
                    sw.Stop();
                    return;
                }

                var botUser = await telegramService.GetMeAsync()
                    .ConfigureAwait(false);

                Log.Information("Starting SangMata check..");

                var hitActivity = telegramService.GetChatCache<HitActivity>(fromId.ToString());
                if (hitActivity == null)
                {
                    Log.Information("This may first Hit from User {V}. In {V1}", fromId, sw.Elapsed);

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

                Log.Debug("ZiziMata: {0}", hitActivity.ToJson(true));

                var changesCount = 0;
                var msgBuild = new StringBuilder();

                msgBuild.AppendLine("😽 <b>MataZizi</b>");
                msgBuild.AppendLine($"<b>UserID:</b> {fromId}");

                if (fromUsername != hitActivity.FromUsername)
                {
                    Log.Debug("Username changed detected!");
                    msgBuild.AppendLine($"Mengubah Username menjadi @{fromUsername}");
                    changesCount++;
                }

                if (fromFName != hitActivity.FromFirstName)
                {
                    Log.Debug("First Name changed detected!");
                    msgBuild.AppendLine($"Mengubah nama depan menjadi {fromFName}");
                    changesCount++;
                }

                if (fromLName != hitActivity.FromLastName)
                {
                    Log.Debug("Last Name changed detected!");
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

                Log.Information("MataZizi completed in {0}. Changes: {1}", sw.Elapsed, changesCount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error SangMata");
            }

            sw.Stop();
        }
    }
}