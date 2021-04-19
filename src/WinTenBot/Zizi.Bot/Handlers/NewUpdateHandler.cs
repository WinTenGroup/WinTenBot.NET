using System;
using Humanizer;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers
{
    public class NewUpdateHandler : IUpdateHandler
    {
        private readonly TelegramService _telegramService;
        private readonly AfkService _afkService;
        private readonly AntiSpamService _antiSpamService;
        private readonly SettingsService _settingsService;
        private readonly WordFilterService _wordFilterService;

        private ChatSetting _chatSettings;

        public NewUpdateHandler(
            AfkService afkService,
            AntiSpamService antiSpamService,
            SettingsService settingsService,
            TelegramService telegramService,
            WordFilterService wordFilterService
        )
        {
            _afkService = afkService;
            _antiSpamService = antiSpamService;
            _telegramService = telegramService;
            _settingsService = settingsService;
            _wordFilterService = wordFilterService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            if (_telegramService.Context.Update.ChannelPost != null) return;

            _telegramService.IsTooOld();

            _chatSettings = _telegramService.CurrentSetting;

            Log.Debug("NewUpdate: {V}", _telegramService.ToJson(true));

            // Pre-Task is should be awaited.
            await EnqueuePreTask();

            if (_chatSettings.EnableWarnUsername
                && _telegramService.IsGroupChat())
            {
                Log.Debug("Await next condition 1. is enable Warn Username && is Group Chat..");
                if (!_telegramService.IsNoUsername || _telegramService.MessageOrEdited.Text == null)
                {
                    // Next, do what bot should do.
                    Log.Debug("Await next condition on sub condition 1. is has Username || MessageText == null");
                    await next(context, cancellationToken);
                }
            }
            else if (_telegramService.IsPrivateChat)
            {
                Log.Debug("Await next condition 2. if private chat");
                await next(context, cancellationToken);
            }
            else
            {
                Log.Debug("Await next else condition..");
                await next(context, cancellationToken);
            }

            //
            // if (_telegramService.MessageOrEdited.Text == null)
            // {
            //     await next(context, cancellationToken);
            // }

            // Last, do additional task which bot may do
            await RunPostTasks();
        }

        private async Task EnqueuePreTask()
        {
            Log.Information("Enqueue pre tasks");
            var sw = Stopwatch.StartNew();

            var message = _telegramService.AnyMessage;
            var callbackQuery = _telegramService.CallbackQuery;

            var shouldAwaitTasks = new List<Task>();

            if (!_telegramService.IsPrivateChat)
            {
                shouldAwaitTasks.Add(_telegramService.EnsureChatRestrictionAsync());

                if (message?.Text != null)
                {
                    shouldAwaitTasks.Add(_telegramService.CheckGlobalBanAsync());
                    shouldAwaitTasks.Add(_telegramService.CheckCasBanAsync());
                    shouldAwaitTasks.Add(_telegramService.CheckSpamWatchAsync());
                    shouldAwaitTasks.Add(_telegramService.CheckUsernameAsync());
                }
            }

            if (callbackQuery == null)
            {
                shouldAwaitTasks.Add(ScanMessageAsync());
                // shouldAwaitTasks.Add(_telegramService.CheckMessageAsync());
            }

            Log.Debug("Awaiting should await task..");

            await Task.WhenAll(shouldAwaitTasks.ToArray());

            Log.Debug("All preTask completed in {Elapsed}", sw.Elapsed);
            sw.Stop();
        }

        private async Task RunPostTasks()
        {
            var nonAwaitTasks = new List<Task>();

            //Exec nonAwait Tasks
            Log.Debug("Running nonAwait task..");

            nonAwaitTasks.Add(EnsureChatHealthAsync());
            nonAwaitTasks.Add(AfkCheck());
            nonAwaitTasks.Add(CheckMataZiziAsync());

            //nonAwaitTasks.Add(_telegramService.EnsureChatHealthAsync());
            // nonAwaitTasks.Add(_telegramService.AfkCheckAsync());
            // nonAwaitTasks.Add(_telegramService.CheckMataZiziAsync());
            // nonAwaitTasks.Add(_telegramService.HitActivityAsync());

            await Task.WhenAll(nonAwaitTasks.ToArray());
        }

            if (message.Text != null)
            {
                // nonAwaitTasks.Add(_telegramService.FindNotesAsync());
                // nonAwaitTasks.Add(_telegramService.FindTagsAsync());
            }

            nonAwaitTasks.Add(_telegramService.CheckMataZiziAsync());
            nonAwaitTasks.Add(_telegramService.HitActivityAsync());

            // This List Task should not await.
            await Task.WhenAll(nonAwaitTasks.ToArray());
        }

        private async Task ScanMessageAsync()
        {
            var message = _telegramService.MessageOrEdited;
            var msgId = message.MessageId;

            var text = message.Text ?? message.Caption;
            if (text.IsNullOrEmpty())
            {
                Log.Information("No message Text for scan..");
            }
            else
            {
                var result = await _wordFilterService.IsMustDelete(text);
                var isMustDelete = result.IsSuccess;

                if (isMustDelete)
                {
                    Log.Information("Starting scan image if available..");
                }

                Log.Information("Message {MsgId} IsMustDelete: {IsMustDelete}", msgId, isMustDelete);

                if (isMustDelete)
                {
                    Log.Debug("Result: {V}", result.ToJson(true));
                    var note = "Pesan di Obrolan di hapus karena terdeteksi filter Kata.\n" + result.Notes;
                    await _telegramService.SendEventAsync(note);

                    await _telegramService.DeleteAsync(message.MessageId);
                }
            }
        }

        #region Post Task

        private async Task AfkCheck()
        {
            var sw = Stopwatch.StartNew();

            try
            {
                Log.Information("Starting check AFK");

                var message = _telegramService.MessageOrEdited;

                if (!_chatSettings.EnableAfkStat)
                {
                    Log.Information("Afk Stat is disabled in this Group!");
                    return;
                }

                if (message.Text.StartsWith("/afk")) return;

                if (message.ReplyToMessage != null)
                {
                    var repMsg = message.ReplyToMessage;
                    var repFromId = repMsg.From.Id;

                    var isAfkReply = await _afkService.IsExistInDb(repFromId);
                    if (isAfkReply)
                    {
                        var repNameLink = repMsg.GetFromNameLink();
                        await _telegramService.SendTextAsync($"{repNameLink} sedang afk");
                    }
                }

                var fromId = message.From.Id;
                var isAfk = await _afkService.IsExistInDb(fromId);
                if (isAfk)
                {
                    var nameLink = message.GetFromNameLink();
                    var currentAfk = await _afkService.GetAfkById(fromId);

                    if (currentAfk.IsAfk)
                    {
                        await _telegramService.SendTextAsync($"{nameLink} sudah tidak afk");
                    }

                    var data = new Dictionary<string, object>
                    {
                        {"chat_id", message.Chat.Id},
                        {"user_id", message.From.Id},
                        {"is_afk", 0},
                        {"afk_reason", ""},
                        {"afk_end", DateTime.Now}
                    };

                    await _afkService.SaveAsync(data);
                    await _afkService.UpdateAfkByIdCacheAsync(fromId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured when run {V}", nameof(AfkCheck).Humanize());
            }

            Log.Debug("AFK check completed. In {Elapsed}", sw.Elapsed);
            sw.Stop();
        }

        public async Task CheckMataZiziAsync()
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var message = _telegramService.MessageOrEdited;
                var fromId = message.From.Id;
                var fromUsername = message.From.Username;
                var fromFName = message.From.FirstName;
                var fromLName = message.From.LastName;
                var chatId = message.Chat.Id;

                var chatSettings = _telegramService.CurrentSetting;
                if (!chatSettings.EnableZiziMata)
                {
                    Log.Information("MataZizi is disabled in this Group!. Completed in {Elapsed}", sw.Elapsed);
                    sw.Stop();
                    return;
                }

                var botUser = await _telegramService.GetMeAsync();

                Log.Information("Starting SangMata check..");

                var hitActivity = _telegramService.GetChatCache<HitActivity>(fromId.ToString());
                if (hitActivity == null)
                {
                    Log.Information("This may first Hit from User {V}. In {V1}", fromId, sw.Elapsed);

                    _telegramService.SetChatCache(fromId.ToString(), new HitActivity()
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

                Log.Debug("ZiziMata: {V}", hitActivity.ToJson(true));

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
                    await _telegramService.SendTextAsync(msgBuild.ToString().Trim());

                    _telegramService.SetChatCache(fromId.ToString(), new HitActivity()
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

                Log.Information("MataZizi completed in {Elapsed}. Changes: {ChangesCount}", sw.Elapsed, changesCount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error SangMata");
            }

            sw.Stop();
        }

        private async Task EnsureChatHealthAsync()
        {
            Log.Information("Ensuring chat health..");

            var message = _telegramService.Message;
            var chatId = message.Chat.Id;

            Log.Information("Preparing restore health on chatId {ChatId}..", chatId);
            var data = new Dictionary<string, object>()
            {
                {"chat_id", chatId},
                {"chat_title", message.Chat.Title ?? @"N\A"},
                {"chat_type", message.Chat.Type.Humanize()},
                {"is_admin", _telegramService.IsBotAdmin}
            };

            var saveSettings = await _settingsService.SaveSettingsAsync(data);
            Log.Debug("Ensure Settings: {SaveSettings}", saveSettings);

            await _settingsService.UpdateCacheAsync(chatId);
        }

        #endregion
    }
}