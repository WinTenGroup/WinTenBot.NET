using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using WinTenBot.Common;
using WinTenBot.Services;

namespace WinTenBot.Handlers.Commands.Tags
{
    public class TagsCommand : CommandBase
    {
        private readonly TagsService _tagsService;
        private SettingsService _settingsService;
        private TelegramService _telegramService;

        public TagsCommand()
        {
            _tagsService = new TagsService();
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            Message msg = context.Update.Message;
            _telegramService = new TelegramService(context);
            _settingsService = new SettingsService(msg);

            var id = msg.From.Id;
            var sendText = "Under maintenance";

            await _telegramService.DeleteAsync(msg.MessageId)
                .ConfigureAwait(false);
            await _telegramService.SendTextAsync("🔄 Loading tags..")
                .ConfigureAwait(false);
            var tagsData = await _tagsService.GetTagsByGroupAsync(msg.Chat.Id)
                .ConfigureAwait(false);
            var tagsStr = string.Empty;

            foreach (var tag in tagsData)
            {
                tagsStr += $"#{tag.Tag} ";
            }

            sendText = $"#️⃣<b> {tagsData.Count} Tags</b>\n" +
                       $"\n{tagsStr}";

            await _telegramService.EditAsync(sendText)
                .ConfigureAwait(false);

            //            var jsonSettings = TextHelper.ToJson(currentSetting);
            //            Log.Information($"CurrentSettings: {jsonSettings}");

            // var lastTagsMsgId = int.Parse(currentSetting.Rows[0]["last_tags_message_id"].ToString());

            var currentSetting = await _settingsService.GetSettingByGroup()
                .ConfigureAwait(false);
            var lastTagsMsgId = currentSetting.LastTagsMessageId;
            Log.Information($"LastTagsMsgId: {lastTagsMsgId}");

            if (lastTagsMsgId.ToInt() > 0) await _telegramService.DeleteAsync(lastTagsMsgId.ToInt())
                .ConfigureAwait(false);
            await _tagsService.UpdateCacheAsync(msg)
                .ConfigureAwait(false);
            await _settingsService.UpdateCell("last_tags_message_id", _telegramService.SentMessageId)
                .ConfigureAwait(false);


//            var json = TextHelper.ToJson(tagsData);
            //                Console.WriteLine(json);
        }
    }
}