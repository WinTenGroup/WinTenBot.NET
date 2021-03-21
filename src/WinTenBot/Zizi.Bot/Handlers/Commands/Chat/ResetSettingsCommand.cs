using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;

namespace Zizi.Bot.Handlers.Commands.Chat
{
    public class ResetSettingsCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly SettingsService _settingsService;

        public ResetSettingsCommand(
            SettingsService settingsService,
            TelegramService telegramService
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
            var chat = _telegramService.Message.Chat;
            var chatId = _telegramService.ChatId;

            var adminOrPrivate = _telegramService.IsAdminOrPrivateChat();
            if (!adminOrPrivate)
            {
                Log.Warning("Settings command only for Admin group or Private chat");
                return;
            }

            Log.Information("Initializing reset Settings.");
            await _telegramService.DeleteAsync(msg.MessageId);
            await _telegramService.SendTextAsync("Sedang mengembalikan ke Pengaturan awal");

            var data = new Dictionary<string, object>()
            {
                ["chat_id"] = chat.Id,
                ["chat_title"] = chat.Title,
                ["enable_afk_stats"] = 1,
                ["enable_anti_malfiles"] = 1,
                ["enable_fed_cas_ban"] = 1,
                ["enable_fed_es2_ban"] = 1,
                ["enable_fed_spamwatch"] = 1,
                ["enable_url_filtering"] = 1,
                ["enable_human_verification"] = 1,
                ["enable_reply_notification"] = 1,
                ["enable_warn_username"] = 1,
                ["enable_word_filter_group"] = 1,
                ["enable_word_filter_global"] = 1,
                ["enable_welcome_message"] = 1,
            };

            var update = await _settingsService.SaveSettingsAsync(data);
            await _settingsService.UpdateSettingsCache(chatId);
            Log.Information("Result: {Update}", update);

            await _telegramService.EditAsync("Pengaturan awal berhasil di kembalikan");
            await _telegramService.DeleteAsync(_telegramService.EditedMessageId, 2000);

            Log.Information("Settings has been reset.");
        }
    }
}