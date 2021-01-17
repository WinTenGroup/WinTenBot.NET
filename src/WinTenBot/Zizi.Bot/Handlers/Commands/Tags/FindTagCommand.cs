using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Enums;
using Zizi.Bot.Models;
using Zizi.Bot.Services;
using Zizi.Bot.Telegram;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Tags
{
    public class FindTagCommand:IUpdateHandler
    {
        private TelegramService _telegramService;
        private TagsService _tagsService;

        public FindTagCommand(TagsService tagsService)
        {
            _tagsService = tagsService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            Log.Information("Finding tag on messages");
            _telegramService = new TelegramService(context);

            var sw = new Stopwatch();
            sw.Start();

            var message = _telegramService.MessageOrEdited;
            var chatSettings = _telegramService.CurrentSetting;
            var chatId = _telegramService.ChatId;

            if (!chatSettings.EnableFindTags)
            {
                Log.Information("Find Tags is disabled in this Group!");
                return;
            }

            // var tagsService = new TagsService();
            // if (!message.Text.Contains("#"))
            // {
            //     Log.Information("Message {0} is not contains any Hashtag.", message.MessageId);
            //     return;
            // }

            var keyCache = $"{chatId.ReduceChatId()}-tags";

            Log.Information("Tags Received..");
            var partsText = message.Text.Split(new char[] {' ', '\n', ','})
                .Where(x => x.Contains("#")).ToArray();

            var allTags = partsText.Length;
            var limitedTags = partsText.Take(5).ToArray();
            var limitedCount = limitedTags.Length;

            Log.Debug("AllTags: {0}", allTags.ToJson(true));
            Log.Debug("First 5: {0}", limitedTags.ToJson(true));
            //            int count = 1;

            // var tags = MonkeyCacheUtil.Get<IEnumerable<CloudTag>>(keyCache);
            var tags = (await _tagsService.GetTagsByGroupAsync(chatId)).ToList();
            foreach (var split in limitedTags)
            {
                Log.Information("Processing : {0} => ", split);
                var trimTag = split.TrimStart('#');

                var tagData = tags.FirstOrDefault(x => x.Tag == trimTag);

                if (tagData == null)
                {
                    Log.Debug("Tag {0} is not found.", trimTag);
                    continue;
                }
                // var tagData = await tagsService.GetTagByTag(message.Chat.Id, trimTag)
                //     .ConfigureAwait(false);

                Log.Debug("Data of tag: {0} => {1}", trimTag, tagData.ToJson(true));

                var content = tagData.Content;
                var buttonStr = tagData.BtnData;
                var typeData = tagData.TypeData;
                var idData = tagData.FileId;

                InlineKeyboardMarkup buttonMarkup = null;
                if (!buttonStr.IsNullOrEmpty())
                {
                    buttonMarkup = buttonStr.ToReplyMarkup(2);
                }

                if (typeData != MediaType.Unknown)
                {
                    await _telegramService.SendMediaAsync(idData, typeData, content, buttonMarkup)
                        .ConfigureAwait(false);
                }
                else
                {
                    await _telegramService.SendTextAsync(content, buttonMarkup)
                        .ConfigureAwait(false);
                }
            }

            if (allTags > limitedCount)
            {
                await _telegramService.SendTextAsync("Due performance reason, we limit 5 batch call tags")
                    .ConfigureAwait(false);
            }

            Log.Information("Find Tags completed in {0}", sw.Elapsed);
            sw.Stop();
        }

        private void ProcessingMessage()
        {

        }
    }
}