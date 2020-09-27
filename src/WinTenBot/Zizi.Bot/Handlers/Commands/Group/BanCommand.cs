using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Group
{
    public class BanCommand : CommandBase
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

                    isKicked = await _telegramService.KickMemberAsync(target)
                        .ConfigureAwait(false);

                    if (isKicked)
                    {
                        sendText = $"{target} berhasil di blokir ";

                        sendText += idTarget == fromId ? $"oleh Self-ban" : $".";
                    }
                    else
                    {
                        sendText = $"Gagal memblokir {idTarget}";
                    }

                    await _telegramService.AppendTextAsync(sendText)
                        .ConfigureAwait(false);
                }

                if (isKicked)
                {
                    await _telegramService.AppendTextAsync($"Sebanyak {kickTargets.Count} berhasil di blokir.")
                        .ConfigureAwait(false);
                }
                else
                {
                    await _telegramService.AppendTextAsync("Gagal memblokir bbrp anggota")
                        .ConfigureAwait(false);
                }
            }
            else if (kickTargets[0].Id == fromId)
            {
                var idTarget = kickTargets[0];
                var isKicked = false;

                isKicked = await _telegramService.KickMemberAsync(idTarget)
                    .ConfigureAwait(false);
                if (isKicked)
                {
                    var sendText = $"{idTarget} berhasil di blokir ";
                    sendText += idTarget.Id == fromId ? $"oleh Self-ban" : $".";
                    await _telegramService.AppendTextAsync(sendText)
                        .ConfigureAwait(false);
                }
                else
                {
                    var sendTexts = $"Blokir {idTarget} gagal.";
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