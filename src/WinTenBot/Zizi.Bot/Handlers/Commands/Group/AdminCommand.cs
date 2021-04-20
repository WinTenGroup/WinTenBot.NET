using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;
using Zizi.Core.Utils.Text;

namespace Zizi.Bot.Handlers.Commands.Group
{
    public class AdminCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public AdminCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            if (_telegramService.IsPrivateChat)
            {
                Log.Warning("Get admin list only for group");
                return;
            }

            await _telegramService.SendTextAsync("🍽 Loading..");

            var admins = await _telegramService.GetChatAdmin();

            var creatorStr = string.Empty;
            var sbAdmin = new StringBuilder();

            int number = 1;
            foreach (var admin in admins)
            {
                var user = admin.User;
                var nameLink = user.Id.GetNameLink((user.FirstName + " " + user.LastName).Trim());
                if (admin.Status == ChatMemberStatus.Creator)
                {
                    creatorStr = nameLink;
                }
                else
                {
                    sbAdmin.AppendLine($"{number++}. {nameLink}");
                }
            }

            var sendText = $"👤 <b>Creator</b>" +
                           $"\n└ {creatorStr}" +
                           $"\n" +
                           $"\n👥️ <b>Administrators</b>" +
                           $"\n{sbAdmin.ToTrimmedString()}";

            await _telegramService.EditAsync(sendText);

            await _telegramService.UpdateCacheAdminAsync();
        }
    }
}