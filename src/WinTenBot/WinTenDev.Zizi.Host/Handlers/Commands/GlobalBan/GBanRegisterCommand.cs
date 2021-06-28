using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Host.Telegram;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Services;

namespace WinTenDev.Zizi.Host.Handlers.Commands.GlobalBan
{
    public class GBanRegisterCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly GlobalBanService _globalBanService;

        public GBanRegisterCommand(
            TelegramService telegramService,
            GlobalBanService globalBanService
        )
        {
            _telegramService = telegramService;
            _globalBanService = globalBanService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var message = _telegramService.Message;
            var userId = message.From.Id;

            if (!await _telegramService.IsBeta()) return;

            await _telegramService.SendTextAsync("Sedang memeriksa persyaratan");

            if (_telegramService.IsPrivateChat)
            {
                await _telegramService.EditAsync("Register Fed ES2 tidak dapat dilakukan di Private Chat.");
                return;
            }

            if (!await _telegramService.IsAdminGroup())
            {
                await _telegramService.EditAsync("Hanya admin yang dapat register ke Fed ES2.");
                return;
            }

            var memberCount = await _telegramService.GetMemberCount();
            if (memberCount < 197)
            {
                await _telegramService.EditAsync("Jumlah member di Grup ini kurang dari persyaratan minimum.");
                return;
            }

            // await _telegramService.RegisterGBanAdmin();

            if (message.ReplyToMessage != null)
            {
                var repMsg = message.ReplyToMessage;
                if (repMsg.From.IsBot)
                {
                    await _telegramService.EditAsync("Tidak dapat meregister Bot menjadi admin ES2");
                    return;
                }

                userId = message.ReplyToMessage.From.Id;
            }

            var adminItem = new GBanAdminItem()
            {
                Username = message.From.Username,
                UserId = userId,
                PromotedBy = message.From.Id,
                PromotedFrom = message.Chat.Id,
                CreatedAt = DateTime.Now,
                IsBanned = false,
            };

            var isRegistered = await _globalBanService.IsGBanAdminAsync(userId);
            if (isRegistered)
            {
                await _telegramService.EditAsync($"Sepertinya UserID {adminItem.UserId} sudah menjadi Admin Fed");
                return;
            }

            await _telegramService.EditAsync("Sedang meregister ke GBan Admin");
            await _globalBanService.SaveAdminGban(adminItem);

            await _telegramService.EditAsync("Selesai");
        }
    }
}