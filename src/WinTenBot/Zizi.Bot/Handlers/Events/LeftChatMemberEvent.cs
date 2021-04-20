using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Providers;
using Zizi.Bot.Services.Features;

namespace Zizi.Bot.Handlers.Events
{
    public class LeftChatMemberEvent : IUpdateHandler
    {
        private readonly TelegramService _telegramService;

        public LeftChatMemberEvent(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            var leftMember = msg.LeftChatMember;
            var leftUserId = leftMember.Id;
            var isBan = await leftUserId.CheckGBan();

            if (!isBan)
            {
                Log.Information("Left Chat Members...");

                var chatTitle = msg.Chat.Title;
                var leftChatMember = msg.LeftChatMember;
                var leftFullName = leftChatMember.FirstName;

                var sendText = $"Sampai jumpa lagi {leftFullName} " +
                               $"\nKami di <b>{chatTitle}</b> menunggumu kembali.. :(";

                await _telegramService.SendTextAsync(sendText);
            }
            else
            {
                Log.Information("Left Message ignored because {LeftMember} is Global Banned", leftMember);
            }
        }
    }
}