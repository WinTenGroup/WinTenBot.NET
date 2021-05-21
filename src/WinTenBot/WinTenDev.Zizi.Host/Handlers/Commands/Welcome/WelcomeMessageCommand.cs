using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Telegram;
using WinTenDev.Zizi.Host.Telegram;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Welcome
{
    public class WelcomeMessageCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly SettingsService _settingsService;

        public WelcomeMessageCommand(
            TelegramService telegramService,
            SettingsService settingsService
        )
        {
            _telegramService = telegramService;
            _settingsService = settingsService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = _telegramService.Message;
            var chatId = _telegramService.ChatId;

            if (_telegramService.IsPrivateChat)
            {
                return;
            }

            if (!await _telegramService.IsAdminGroup())
            {
                return;
            }

            var columnTarget = $"welcome_message";
            var data = msg.Text.GetTextWithoutCmd();

            if (msg.ReplyToMessage != null)
            {
                data = msg.ReplyToMessage.Text;
            }

            if (data.IsNullOrEmpty())
            {
                await _telegramService.SendTextAsync($"Silakan masukan konfigurasi Pesan yang akan di terapkan");
                return;
            }

            await _telegramService.SendTextAsync($"Sedang menyimpan Welcome Message..");

            await _settingsService.UpdateCell(chatId, columnTarget, data);

            await _telegramService.EditAsync($"Welcome Button berhasil di simpan!" +
                                             $"\nKetik /welcome untuk melihat perubahan");

            Log.Information("Success save welcome Message on {ChatId}.", chatId);
        }
    }
}