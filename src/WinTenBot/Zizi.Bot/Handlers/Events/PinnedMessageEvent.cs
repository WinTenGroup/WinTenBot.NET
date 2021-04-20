using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Events
{
    public class PinnedMessageEvent : IUpdateHandler
    {
        private readonly TelegramService _telegramService;

        public PinnedMessageEvent(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;

            var pinnedMsg = msg.PinnedMessage;
            var sendText = $"📌 Pesan di sematkan baru!" +
                           $"\nPengirim: {pinnedMsg.GetFromNameLink()}" +
                           $"\nPengepin: {msg.GetFromNameLink()}";

            await _telegramService.SendTextAsync(sendText);
        }
    }
}