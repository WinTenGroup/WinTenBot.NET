using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Telegram;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Chat
{
    /// <summary>
    /// Get simple identity of Chat and User
    /// </summary>
    public class IdCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        /// <summary>
        /// ID Constructor
        /// </summary>
        /// <param name="telegramService"></param>
        public IdCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        /// <summary>
        /// Handle get ID command
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = _telegramService.Message;

            var chatTitle = msg.Chat.Title;
            var chatId = msg.Chat.Id;
            var chatType = msg.Chat.Type;

            var userId = msg.From.Id;
            var username = msg.From.Username;
            var name = msg.From.GetFullName();
            var userLang = msg.From.LanguageCode;

            var text = $"👥 <b>{chatTitle}</b>\n" +
                       $"ID: <code>{chatId}</code>\n" +
                       $"Type: <code>{chatType}</code>\n\n" +
                       $"👤 <b>{name}</b>\n" +
                       $"ID: <code>{userId}</code>\n" +
                       $"Username: @{username}\n" +
                       $"Language: {userLang.ToUpperCase()}";

            if (msg.ReplyToMessage != null)
            {
                var repMsg = msg.ReplyToMessage;
                var repFullName = repMsg.From.GetFullName();

                text += $"\n\n👤 <b>{repFullName}</b>" +
                        $"\nID: <code>{repMsg.From.Id}</code>" +
                        $"\nUsername: @{repMsg.From.Username}" +
                        $"\nLanguage: {repMsg.From.LanguageCode}";
            }

            await _telegramService.SendTextAsync(text);
        }
    }
}