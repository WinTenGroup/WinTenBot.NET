using System.Threading;
using System.Threading.Tasks;
using SqlKata.Execution;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Enums;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Core
{
    public class BackupCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly QueryFactory _queryFactory;

        public BackupCommand(
        QueryFactory queryFactory,
        TelegramService telegramService
        )
        {
            _queryFactory = queryFactory;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var isSudoer = _telegramService.IsSudoer();
            if (!isSudoer) return;

            var message = _telegramService.AnyMessage;
            var chatId = _telegramService.ChatId;

            await _telegramService.SendTextAsync("🔄 Sedang mencadangkan..");
            var tags = await _queryFactory.BackupAllTags();

            await _telegramService.EditAsync("✅ Selesai mencadangkan.");
            await _telegramService.DeleteAsync();

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Tags", "backup tags"),
                    InlineKeyboardButton.WithCallbackData("Settings", "backup settings")
                }
            });

            await _telegramService.SendTextAsync("asd", replyMarkup: inlineKeyboard);

            await _telegramService.SendMediaAsync(tags, MediaType.LocalDocument, "#backup #tags");
        }
    }
}