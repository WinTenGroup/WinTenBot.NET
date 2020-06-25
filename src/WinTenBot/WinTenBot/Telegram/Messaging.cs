using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WinTenBot.Common;
using WinTenBot.Enums;
using WinTenBot.IO;
using WinTenBot.Model;
using WinTenBot.Providers;
using WinTenBot.Services;
using WinTenBot.Tools;
using WinTenBot.Tools.GoogleCloud;

namespace WinTenBot.Telegram
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
            if (withoutCmd && message.StartsWith("/"))
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

            Log.Information($"MessageLink: {messageLink}");
            return messageLink;
        }

        private static async Task<bool> IsMustDelete(string words)
        {
            var isMust = false;
            var query = await new Query("word_filter")
                .ExecForSqLite(true)
                .GetAsync()
                .ConfigureAwait(false);

            var mappedWords = query.ToJson().MapObject<List<WordFilter>>();

            var partedWord = words.Split(new[] {'\n', '\r', ' ', '\t'},
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in partedWord)
            {
                var forCompare = word;
                if (forCompare.IsValidUrl()) forCompare = forCompare.ParseUrl().Path;
                forCompare = forCompare.ToLowerCase().CleanExceptAlphaNumeric();

                foreach (var wordFilter in mappedWords)
                {
                    var isGlobal = wordFilter.IsGlobal;
                    var isDeep = wordFilter.DeepFilter;

                    var forFilter = wordFilter.Word.ToLowerCase();
                    if (forFilter.EndsWith("*"))
                    {
                        var distinctChar = forCompare.DistinctChar();
                        forFilter = forFilter.CleanExceptAlphaNumeric();
                        isMust = forCompare.Contains(forFilter);
                        $"'{forCompare}' LIKE '{forFilter}' ? {isMust}. Global: {isGlobal}".LogDebug();

                        if (!isMust)
                        {
                            isMust = distinctChar.Contains(forFilter);
                            $"'{distinctChar}' LIKE '{forFilter}' ? {isMust}. Global: {isGlobal}".LogDebug();
                        }
                    }
                    else
                    {
                        forFilter = wordFilter.Word.ToLowerCase().CleanExceptAlphaNumeric();
                        if (forCompare == forFilter) isMust = true;
                        $"'{forCompare}' == '{forFilter}' ? {isMust}. Deep: {isDeep}, Global: {isGlobal}".LogDebug();
                    }

                    if (!isMust) continue;
                    Log.Information("Break check now!");
                    break;
                }

                if (!isMust) continue;
                Log.Information("Should break!");
                break;
            }

            return isMust;
        }

        private static async Task ScanMessageAsync(this TelegramService telegramService)
        {
            var message = telegramService.MessageOrEdited;
            var msgId = message.MessageId;

            var text = message.Text ?? message.Caption;
            if (!text.IsNullOrEmpty())
            {
                var isMustDelete = await IsMustDelete(text)
                    .ConfigureAwait(false);

                Log.Information($"Message {msgId} IsMustDelete: {isMustDelete}");

                if (isMustDelete)
                    await telegramService.DeleteAsync(message.MessageId)
                        .ConfigureAwait(false);
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
            var msgId = message.MessageId;

            if (message.Photo != null)
            {
                Log.Information("");

                var fileName = $"{chatId}/ocr-{msgId}.jpg";
                Log.Information("Preparing photo");
                var savedFile = await telegramService.DownloadFileAsync(fileName)
                    .ConfigureAwait(false);

                // var ocr = await TesseractProvider.OcrSpace(savedFile)
                //     .ConfigureAwait(false);
                var ocr = GoogleVision.ScanText(savedFile);
                // var safeSearch = GoogleVision.SafeSearch(savedFile);

                Log.Information("Scanning message..");
                var isMustDelete = await IsMustDelete(ocr)
                    .ConfigureAwait(false);

                Log.Information($"Message {message.MessageId} IsMustDelete: {isMustDelete}");

                if (isMustDelete)
                    await telegramService.DeleteAsync(message.MessageId)
                        .ConfigureAwait(false);

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

        public static async Task FindTagsAsync(this TelegramService telegramService)
        {
            var message = telegramService.MessageOrEdited;
            var chatSettings = telegramService.CurrentSetting;
            if (!chatSettings.EnableFindTags)
            {
                Log.Information("Find Tags is disabled in this Group!");
                return;
            }

            var tagsService = new TagsService();
            if (!message.Text.Contains("#"))
            {
                Log.Information($"Message {message.MessageId} is not contains any Hashtag.");
                return;
            }

            Log.Information("Tags Received..");
            var partsText = message.Text.Split(new char[] {' ', '\n', ','})
                .Where(x => x.Contains("#")).ToArray();

            var allTags = partsText.Length;
            var limitedTags = partsText.Take(5).ToArray();
            var limitedCount = limitedTags.Length;

            Log.Information($"AllTags: {allTags.ToJson(true)}");
            Log.Information($"First 5: {limitedTags.ToJson(true)}");
            //            int count = 1;
            foreach (var split in limitedTags)
            {
                var trimTag = split.TrimStart('#');
                Log.Information($"Processing : {trimTag}");

                var tagData = await tagsService.GetTagByTag(message.Chat.Id, trimTag)
                    .ConfigureAwait(false);
                Log.Information($"Data of tag: {trimTag} {tagData.ToJson(true)}");

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
        }
    }
}