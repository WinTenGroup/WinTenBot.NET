using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Types.ReplyMarkups;
using WinTenDev.Zizi.Host.Tools.GoogleCloud;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.IO;
using WinTenDev.Zizi.Utils.Providers;
using WinTenDev.Zizi.Utils.Telegram;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Host.Telegram
{
    public static class Messaging
    {


        public static async Task<List<WordFilter>> GetWordFiltersBase()
        {
            // var query = await new Query("word_filter")
            //     .ExecForSqLite(true)
            //     .Where("is_global", 1)
            //     .GetAsync();
            //
            // var mappedWords = query.ToJson().MapObject<List<WordFilter>>();


            // var jsonWords = "local-words".OpenJson();
            // var listWords = (await jsonWords.GetCollectionAsync<WordFilter>()
            // )
            // .AsQueryable()
            // .Where(x => x.IsGlobal)
            // .ToList();

            // jsonWords.Dispose();

            var collection = await LiteDbProvider.GetCollectionsAsync<WordFilter>();

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

            var listWords = await GetWordFiltersBase();

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
                        Log.Debug("'{0}' == '{1}' ? {2}, Global: {3}", forCompare, forFilter, isShould, isGlobal);
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
                var result = await IsMustDelete(text);
                var isMustDelete = result.IsSuccess;

                Log.Information("Message {0} IsMustDelete: {1}", msgId, isMustDelete);

                if (isMustDelete)
                {
                    Log.Debug("Result: {0}", result.ToJson(true));
                    var note = "Pesan di Obrolan di hapus karena terdeteksi filter Kata.\n" + result.Notes;
                    await telegramService.SendEventAsync(note);

                    await telegramService.DeleteAsync(message.MessageId);
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

                var isSafe = await telegramService.IsSafeMemberAsync();
                if (isSafe)
                {
                    Log.Information("Scan photo skipped because UserId {0} is SafeMember", fromId);
                    return;
                }

                var fileName = $"{chatId}/ocr-{msgId}.jpg";
                Log.Information("Preparing photo");
                var savedFile = await telegramService.DownloadFileAsync(fileName);

                // var ocr = await TesseractProvider.OcrSpace(savedFile)
                //     ;
                var ocr = GoogleVision.ScanText(savedFile);

                // var safeSearch = GoogleVision.SafeSearch(savedFile);
                // Log.Debug($"SafeSearch: {safeSearch.ToJson(true)}");

                Log.Information("Scanning message..");
                var result = await IsMustDelete(ocr);
                var isMustDelete = result.IsSuccess;

                Log.Information("Message {0} IsMustDelete: {1}", message.MessageId, isMustDelete);

                if (isMustDelete)
                {
                    Log.Debug("Result: {0}", result.ToJson(true));
                    var note = "Pesan gambar di Obrolan di hapus karena terdeteksi filter Kata.\n" + result.Notes;
                    await telegramService.SendEventAsync(note);

                    await telegramService.DeleteAsync(message.MessageId);
                }
                else
                {
                    await telegramService.VerifySafeMemberAsync();
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

                await Task.WhenAll(listAction);
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

                var selectedNotes = await notesService.GetNotesBySlug(message.Chat.Id, message.Text);
                if (selectedNotes.Count > 0)
                {
                    var content = selectedNotes[0].Content;
                    var btnData = selectedNotes[0].BtnData;
                    if (!btnData.IsNullOrEmpty())
                    {
                        inlineKeyboardMarkup = btnData.ToReplyMarkup(2);
                    }

                    await telegramService.SendTextAsync(content, inlineKeyboardMarkup);

                    foreach (var note in selectedNotes)
                    {
                        Log.Debug("List Notes: {V}", note.ToJson(true));
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

    }
}