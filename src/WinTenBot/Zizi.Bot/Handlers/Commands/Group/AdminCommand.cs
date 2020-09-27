using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Services;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Commands.Group
{
    public class AdminCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            var msg = context.Update.Message;
            _telegramService = new TelegramService(context);
            var client = _telegramService.Client;

            await _telegramService.SendTextAsync("🍽 Loading..").ConfigureAwait(false);
            //            var admins = context.Update.Message.Chat.AllMembersAreAdministrators;
            var admins = await client.GetChatAdministratorsAsync(msg.Chat.Id, cancellationToken)
                .ConfigureAwait(false);
            var creatorStr = string.Empty;
            var adminStr = string.Empty;
            int number = 1;
            foreach (var admin in admins)
            {
                var user = admin.User;
                var nameLink = Members.GetNameLink(user.Id, (user.FirstName + " " + user.LastName).Trim());
                if (admin.Status == ChatMemberStatus.Creator)
                {
                    creatorStr = nameLink;
                }
                else
                {
                    adminStr += $"{number++}. {nameLink}\n";
                }

                //                Console.WriteLine(TextHelper.ToJson(admin));
                //                await chatProcessor.EditAsync(TextHelper.ToJson(admin));
            }

            var sendText = $"👤 <b>Creator</b>" +
                           $"\n└ {creatorStr}" +
                           $"\n" +
                           $"\n👥️ <b>Administrators</b>" +
                           $"\n{adminStr}";

            await _telegramService.EditAsync(sendText)
                .ConfigureAwait(false);
        }
    }
}