using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Events
{
    public class LeftChatMemberEvent : IUpdateHandler
    {
        private readonly TelegramService _telegramService;
        private readonly AntiSpamService _antiSpamService;

        public LeftChatMemberEvent(
            TelegramService telegramService,
            AntiSpamService antiSpamService
        )
        {
            _telegramService = telegramService;
            _antiSpamService = antiSpamService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;
            var leftMember = msg.LeftChatMember;
            var leftUserId = leftMember.Id;
            var checkSpam = await _antiSpamService.CheckSpam(leftUserId);
            // var isBan = await leftUserId.CheckGBan();

            if (checkSpam.IsAnyBanned)
            {
                Log.Information("Left Message ignored because {LeftMember} is Global Banned", leftMember);
                return;
            }

            Log.Information("Left Chat Members...");

            var chatTitle = msg.Chat.Title;
            var leftChatMember = msg.LeftChatMember;
            var leftFullName = leftChatMember.FirstName;

            var sendText = $"Sampai jumpa lagi {leftFullName} " +
                           $"\nKami di <b>{chatTitle}</b> menunggumu kembali.. :(";

            await _telegramService.SendTextAsync(sendText);
        }
    }
}