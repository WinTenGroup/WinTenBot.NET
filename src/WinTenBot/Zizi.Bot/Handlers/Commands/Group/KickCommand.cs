using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Group
{
    public class KickCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            var kickTargets = new List<User>();

            var msg = context.Update.Message;
            Message repMsg = null;
            var fromId = msg.From.Id;
            // var idTargets = new List<int>();

            kickTargets.Add(msg.From);

            if (msg.ReplyToMessage != null)
            {
                repMsg = msg.ReplyToMessage;
                // idTarget = repMsg.From.id;
                kickTargets.Add(repMsg.From);

                if (repMsg.NewChatMembers != null)
                {
                    kickTargets.Clear();
                    var userTargets = repMsg.NewChatMembers;
                    kickTargets.AddRange(userTargets);

                    // foreach (var target in userTargets)
                    // {
                    //     idTargets.Add(target.Id);
                    // }
                }
            }

            await _telegramService.DeleteAsync(msg.MessageId)
                .ConfigureAwait(false);

            var isAdmin = await _telegramService.IsAdminGroup()
                .ConfigureAwait(false);

            if (kickTargets[0].Id != msg.From.Id && isAdmin)
            {
                var isKicked = false;
                foreach (var target in kickTargets)
                {
                    var idTarget = target.Id;
                    var sendText = string.Empty;

                    // await _chatProcessor.AppendTextAsync($"Sedang menendang {idTarget}");
                    isKicked = await _telegramService.KickMemberAsync(target)
                        .ConfigureAwait(false);
                    await _telegramService.UnbanMemberAsync(target)
                        .ConfigureAwait(false);

                    if (isKicked)
                    {
                        sendText = $"{target} berhasil di tendank ";

                        sendText += idTarget == fromId ? $"oleh Self-kick" : $".";
                    }
                    else
                    {
                        sendText = $"Gagal menendang {idTarget}";
                    }

                    await _telegramService.AppendTextAsync(sendText)
                        .ConfigureAwait(false);
                }

                if (isKicked)
                {
                    await _telegramService.AppendTextAsync($"Sebanyak {kickTargets.Count} berhasil di tendang.")
                        .ConfigureAwait(false);
                }
                else
                {
                    await _telegramService.AppendTextAsync("Gagal menendang bbrp anggota")
                        .ConfigureAwait(false);
                }
            }
            else if (kickTargets[0].Id == fromId)
            {
                var idTarget = kickTargets[0];
                var isKicked = false;
                // await _chatProcessor.AppendTextAsync($"Sedang menendang {idTarget}");
                isKicked = await _telegramService.KickMemberAsync(idTarget)
                    .ConfigureAwait(false);
                if (isKicked)
                {
                    var sendText = $"{idTarget} berhasil di tendang ";
                    sendText += idTarget.Id == fromId ? $"oleh Self-kick" : $".";
                    await _telegramService.AppendTextAsync(sendText)
                        .ConfigureAwait(false);
                }
                else
                {
                    var sendTexts = $"Tendang {idTarget} gagal.";
                    await _telegramService.SendTextAsync(sendTexts)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                await _telegramService.SendTextAsync("Hanya admin yang bisa mengeksekusi")
                    .ConfigureAwait(false);
            }
        }
    }
}