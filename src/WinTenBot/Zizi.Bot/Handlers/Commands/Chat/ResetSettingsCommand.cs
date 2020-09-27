using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Chat
{
    public class ResetSettingsCommand : CommandBase
    {
        private TelegramService _telegramService;
        private SettingsService _settingsService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            _settingsService = new SettingsService(_telegramService.Message);
            var chat = _telegramService.Message.Chat;
            var msg = _telegramService.Message;

            var adminOrPrivate = await _telegramService.IsAdminOrPrivateChat()
                .ConfigureAwait(false);
            if (adminOrPrivate)
            {
                Log.Information("Initializing reset Settings.");
                await _telegramService.DeleteAsync(msg.MessageId)
                    .ConfigureAwait(false);
                await _telegramService.SendTextAsync("Sedang mengembalikan ke Pengaturan awal")
                    .ConfigureAwait(false);

                var data = new Dictionary<string, object>()
                {
                    ["chat_id"] = chat.Id,
                    ["chat_title"] = chat.Title,
                    ["enable_afk_stats"] = 1,
                    ["enable_anti_malfiles"] = 1,
                    ["enable_fed_cas_ban"] = 1,
                    ["enable_fed_es2"] = 1,
                    ["enable_fed_spamwatch"] = 1,
                    ["enable_url_filtering"] = 1,
                    ["enable_human_verification"] = 1,
                    ["enable_reply_notification"] = 1,
                    ["enable_warn_username"] = 1,
                    ["enable_word_filter_group_wide"] = 1,
                    ["enable_word_filter_per_group"] = 1,
                    ["enable_welcome_message"] = 1,
                };

                var update = await _settingsService.SaveSettingsAsync(data)
                    .ConfigureAwait(false);
                Log.Information($"Result: {update}");

                await _telegramService.EditAsync("Pengaturan awal berhasil di kembalikan")
                    .ConfigureAwait(false);
                await _telegramService.DeleteAsync(_telegramService.EditedMessageId, 2000)
                    .ConfigureAwait(false);
                Log.Information("Settings has been reset.");
            }
        }
    }
}