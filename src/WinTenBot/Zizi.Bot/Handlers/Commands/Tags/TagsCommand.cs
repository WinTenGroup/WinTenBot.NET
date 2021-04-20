using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Core.Utils.Text;

namespace Zizi.Bot.Handlers.Commands.Tags
{
    public class TagsCommand : CommandBase
    {
        private readonly TagsService _tagsService;
        private readonly SettingsService _settingsService;
        private readonly TelegramService _telegramService;

        public TagsCommand(
            TelegramService telegramService,
            TagsService tagsService,
            SettingsService settingsService
        )
        {
            _telegramService = telegramService;
            _tagsService = tagsService;
            _settingsService = settingsService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);
            var chatId = _telegramService.ChatId;
            var msg = context.Update.Message;

            var sendText = "Under maintenance";

            await _telegramService.DeleteAsync(msg.MessageId);
            await _telegramService.SendTextAsync("🔄 Loading tags..");
            var tagsData = (await _tagsService.GetTagsByGroupAsync(chatId)).ToList();
            var tagsStr = new StringBuilder();

            foreach (var tag in tagsData)
            {
                tagsStr.Append($"#{tag.Tag} ");
            }

            sendText = $"#️⃣<b> {tagsData.Count} Tags</b>\n" +
                       $"\n{tagsStr.ToTrimmedString()}";

            await _telegramService.EditAsync(sendText);

            var currentSetting = await _settingsService.GetSettingsByGroup(chatId);
            var lastTagsMsgId = currentSetting.LastTagsMessageId;
            Log.Information("LastTagsMsgId: {LastTagsMsgId}", lastTagsMsgId);

            if (lastTagsMsgId.ToInt() > 0)
                await _telegramService.DeleteAsync(lastTagsMsgId.ToInt());

            await _tagsService.UpdateCacheAsync(chatId);
            await _settingsService.UpdateCell(chatId, "last_tags_message_id", _telegramService.SentMessageId);
        }
    }
}