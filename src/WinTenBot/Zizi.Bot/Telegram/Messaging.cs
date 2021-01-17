using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Enums;
using Zizi.Bot.IO;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;
using Zizi.Bot.Tools.GoogleCloud;

namespace Zizi.Bot.Telegram
{
    public static class Messaging
    {
        public static string GetFileId(this Message message)
        {
            var fileId = "";
            switch (message.Type)
            {
                case MessageType.Document:
                    fileId = message.Document.FileId;
                    break;

                case MessageType.Photo:
                    fileId = message.Photo.Last().FileId;
                    break;

                case MessageType.Video:
                    fileId = message.Video.FileId;
                    break;
            }

            return fileId;
        }

        public static string GetReducedFileId(this Message message)
        {
            return GetFileId(message).Substring(0, 17);
        }

        public static string GetTextWithoutCmd(this string message, bool withoutCmd = true)
        {
            var partsMsg = message.Split(' ');
            var text = message;
            if (withoutCmd && message.StartsWith("/", StringComparison.CurrentCulture))
            {
                text = message.TrimStart(partsMsg[0].ToCharArray());
            }

            return text.Trim();
        }

        public static bool IsNeedRunTasks(this TelegramService telegramService)
        {
            var message = telegramService.Message;

            return message.NewChatMembers == null
                   || message.LeftChatMember == null
                   || !telegramService.IsPrivateChat();
        }

        public static string GetMessageLink(this Message message)
        {
            var chatUsername = message.Chat.Username;
            var messageId = message.MessageId;

            var messageLink = $"https://t.me/{chatUsername}/{messageId}";

            if (chatUsername.IsNullOrEmpty())
            {
                var trimmedChatId = message.Chat.Id.ToString().Substring(4);
                messageLink = $"https://t.me/c/{trimmedChatId}/{messageId}";
            }

            Log.Debug("MessageLink: {0}", messageLink);
            return messageLink;
        }

        public static async Task<List<WordFilter>> GetWordFiltersBase()
        {
            // var query = await new Query("word_filter")
            //     .ExecForSqLite(true)
            //     .Where("is_global", 1)
            //     .GetAsync()
            //     .ConfigureAwait(false);
            //
            // var mappedWords = query.ToJson().MapObject<List<WordFilter>>();


            // var jsonWords = "local-words".OpenJson();
            // var listWords = (await jsonWords.GetCollectionAsync<WordFilter>()
            // .ConfigureAwait(false))
            // .AsQueryable()
            // .Where(x => x.IsGlobal)
            // .ToList();

            // jsonWords.Dispose();

            var collection = await LiteDbProvider.GetCollectionsAsync<WordFilter>()
                .ConfigureAwait(false);

            var listWords = collection.Query()
                .Where(x => x.IsGlobal)
                .ToList();

            return listWords;
        }

        private static async Task<TelegramResult> IsMustDelete(string words)
        {
            var sw = Stopwatch.StartNew();
            var isShould = false;
            var telegramResult = new TelegramResult();

            if (words == null)
            {
                Log.Information("Scan message skipped because Words is null");
                return telegramResult;
            }

            var listWords = await GetWordFiltersBase().ConfigureAwait(false);

            var partedWord = words.Split(new[] {'\n', '\r', ' ', '\t'},
                StringSplitOptions.RemoveEmptyEntries);

            Log.Debug("Message Lists: {0}", partedWord.ToJson(true));
            foreach (var word in partedWord)
            {
                var forCompare = word;
                forCompare = forCompare.ToLowerCase().CleanExceptAlphaNumeric();

                foreach (var wordFilter in listWords)
                {
                    var isGlobal = wordFilter.IsGlobal;
                    var isDeep = wordFilter.DeepFilter;

                    var forFilter = wordFilter.Word.ToLowerCase();
                    if (forFilter.EndsWith("*", StringComparison.CurrentCulture))
                    {
                        var distinctChar = forCompare.DistinctChar();
                        forFilter = forFilter.CleanExceptAlphaNumeric();
                        isShould = forCompare.Contains(forFilter);
                        Log.Debug("'{0}' LIKE '{1}' ? {2}. Global: {3}", forCompare, forFilter, isShould, isGlobal);

                        if (!isShould)
                        {
                            isShould = distinctChar.Contains(forFilter);
                            Log.Debug(messageTemplate: "'{0}' LIKE '{1}' ? {2}. Global: {3}", distinctChar, forFilter, isShould, isGlobal);
                        }
                    }
                    else
                    {
                        forFilter = wordFilter.Word.ToLowerCase().CleanExceptAlphaNumeric();
                        if (forCompare == forFilter) isShould = true;
                        Log.Debug("'{0}' == '{1}' ? {2}. Deep: {3}, Global: {4}", forCompare, forFilter, isShould, isDeep, isGlobal);
                    }

                    if (!isShould) continue;
                    telegramResult.Notes = $"Filter: {forFilter}, Kata: {forCompare}";
                    telegramResult.IsSuccess = true;
                    Log.Information("Break check now!");
                    break;
                }

                if (!isShould) continue;
                Log.Information("Should break!");
                break;
            }

            listWords.Clear();

            Log.Information("Check Message complete in {0}", sw.Elapsed);

            return telegramResult;
        }

        private static async Task ScanMessageAsync(this TelegramService telegramService)
        {
            var message = telegramService.MessageOrEdited;
            var msgId = message.MessageId;

            var text = message.Text ?? message.Caption;
            if (!text.IsNullOrEmpty())
            {
                var result = await IsMustDelete(text).ConfigureAwait(false);
                var isMustDelete = result.IsSuccess;

                Log.Information("Message {0} IsMustDelete: {1}", msgId, isMustDelete);

                if (isMustDelete)
                {
                    Log.Debug("Result: {0}", result.ToJson(true));
                    var note = "Pesan di Obrolan di hapus karena terdeteksi filter Kata.\n" + result.Notes;
                    await telegramService.SendEventAsync(note)
                        .ConfigureAwait(false);

                    await telegramService.DeleteAsync(message.MessageId)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                Log.Information("No message Text for scan.");
            }
        }

        private static async Task ScanPhotoAsync(this TelegramService telegramService)
        {
            var message = telegramService.MessageOrEdited;
            var chatId = message.Chat.Id;
            var fromId = message.From.Id;
            var msgId = message.MessageId;

            if (message.Photo != null)
            {
                Log.Information("");

                var isSafe = await telegramService.IsSafeMemberAsync().ConfigureAwait(false);
                if (isSafe)
                {
                    Log.Information("Scan photo skipped because UserId {0} is SafeMember", fromId);
                    return;
                }

                var fileName = $"{chatId}/ocr-{msgId}.jpg";
                Log.Information("Preparing photo");
                var savedFile = await telegramService.DownloadFileAsync(fileName)
                    .ConfigureAwait(false);

                // var ocr = await TesseractProvider.OcrSpace(savedFile)
                //     .ConfigureAwait(false);
                var ocr = GoogleVision.ScanText(savedFile);

                // var safeSearch = GoogleVision.SafeSearch(savedFile);
                // Log.Debug($"SafeSearch: {safeSearch.ToJson(true)}");

                Log.Information("Scanning message..");
                var result = await IsMustDelete(ocr)
                    .ConfigureAwait(false);
                var isMustDelete = result.IsSuccess;

                Log.Information("Message {0} IsMustDelete: {1}", message.MessageId, isMustDelete);

                if (isMustDelete)
                {
                    Log.Debug("Result: {0}", result.ToJson(true));
                    var note = "Pesan gambar di Obrolan di hapus karena terdeteksi filter Kata.\n" + result.Notes;
                    await telegramService.SendEventAsync(note)
                        .ConfigureAwait(false);

                    await telegramService.DeleteAsync(message.MessageId)
                        .ConfigureAwait(false);
                }
                else
                {
                    await telegramService.VerifySafeMemberAsync()
                        .ConfigureAwait(false);
                }

                savedFile.GetDirectory().RemoveFiles("ocr");
            }
        }

        public static async Task CheckMessageAsync(this TelegramService telegramService)
        {
            try
            {
                Log.Information("Starting check Message");
                var chatSettings = telegramService.CurrentSetting;

                if (!chatSettings.EnableWordFilterGroupWide)
                {
                    Log.Information("Global Word Filter is disabled!");
                    return;
                }

                // var text = message.Text ?? message.Caption;
                // if (!text.IsNullOrEmpty())
                // {
                //     var isMustDelete = await IsMustDelete(text);
                //     Log.Information($"Message {message.MessageId} IsMustDelete: {isMustDelete}");
                //
                //     if (isMustDelete) await telegramService.DeleteAsync(message.MessageId);
                // }
                // else
                // {
                //     Log.Information("No message Text for scan.");
                // }

                var listAction = new List<Task>
                {
                    ScanMessageAsync(telegramService),
                    ScanPhotoAsync(telegramService)
                };

                await Task.WhenAll(listAction).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking message");
            }
        }

        public static async Task FindNotesAsync(this TelegramService telegramService)
        {
            try
            {
                Log.Information("Starting find Notes in Cloud");
                InlineKeyboardMarkup inlineKeyboardMarkup = null;

                var message = telegramService.MessageOrEdited;
                var chatSettings = telegramService.CurrentSetting;
                if (!chatSettings.EnableFindNotes)
                {
                    Log.Information("Find Notes is disabled in this Group!");
                    return;
                }

                var msgText = message.Text;
                if (msgText.IsNullOrEmpty())
                {
                    Log.Information("Message Text should not null or empty");
                    return;
                }

                if (msgText.Contains("\n"))
                {
                    Log.Debug("Slug notes shouldn't multiline.");
                    return;
                }

                var notesService = new NotesService();

                var selectedNotes = await notesService.GetNotesBySlug(message.Chat.Id, message.Text)
                    .ConfigureAwait(false);
                if (selectedNotes.Count > 0)
                {
                    var content = selectedNotes[0].Content;
                    var btnData = selectedNotes[0].BtnData;
                    if (!btnData.IsNullOrEmpty())
                    {
                        inlineKeyboardMarkup = btnData.ToReplyMarkup(2);
                    }

                    await telegramService.SendTextAsync(content, inlineKeyboardMarkup)
                        .ConfigureAwait(false);

                    foreach (var note in selectedNotes)
                    {
                        Log.Debug("List Notes: " + note.ToJson(true));
                    }
                }
                else
                {
                    Log.Debug("No rows result set in Notes");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error when getting Notes");
            }
        }

        [Obsolete("FindTags will moved as command.")]
        public static async Task FindTagsAsync(this TelegramService telegramService)
        {
            var sw = new Stopwatch();
            sw.Start();

            var message = telegramService.MessageOrEdited;
            var chatSettings = telegramService.CurrentSetting;
            var chatId = telegramService.ChatId;

            if (!chatSettings.EnableFindTags)
            {
                Log.Information("Find Tags is disabled in this Group!");
                return;
            }

            // var tagsService = new TagsService();
            if (!message.Text.Contains("#"))
            {
                Log.Information("Message {0} is not contains any Hashtag.", message.MessageId);
                return;
            }

            var keyCache = chatId.ReduceChatId() + "-tags";

            Log.Information("Tags Received..");
            var partsText = message.Text.Split(new char[] {' ', '\n', ','})
                .Where(x => x.Contains("#")).ToArray();

            var allTags = partsText.Length;
            var limitedTags = partsText.Take(5).ToArray();
            var limitedCount = limitedTags.Length;

            Log.Debug("AllTags: {0}", allTags.ToJson(true));
            Log.Debug("First 5: {0}", limitedTags.ToJson(true));
            //            int count = 1;
            var tags = MonkeyCacheUtil.Get<IEnumerable<CloudTag>>(keyCache);
            foreach (var split in limitedTags)
            {
                var trimTag = split.TrimStart('#');
                Log.Information("Processing : {0}", trimTag);

                var tagData = tags.Where(x => x.Tag == trimTag).ToList();
                // var tagData = await tagsService.GetTagByTag(message.Chat.Id, trimTag)
                //     .ConfigureAwait(false);
                Log.Debug("Data of tag: {0} {1}", trimTag, tagData.ToJson(true));

                var content = tagData[0].Content;
                var buttonStr = tagData[0].BtnData;
                var typeData = tagData[0].TypeData;
                var idData = tagData[0].FileId;

                InlineKeyboardMarkup buttonMarkup = null;
                if (!buttonStr.IsNullOrEmpty())
                {
                    buttonMarkup = buttonStr.ToReplyMarkup(2);
                }

                if (typeData != MediaType.Unknown)
                {
                    await telegramService.SendMediaAsync(idData, typeData, content, buttonMarkup)
                        .ConfigureAwait(false);
                }
                else
                {
                    await telegramService.SendTextAsync(content, buttonMarkup)
                        .ConfigureAwait(false);
                }
            }

            if (allTags > limitedCount)
            {
                await telegramService.SendTextAsync("Due performance reason, we limit 5 batch call tags")
                    .ConfigureAwait(false);
            }

            Log.Information("Find Tags completed in {0}", sw.Elapsed);
            sw.Stop();
        }
    }
}