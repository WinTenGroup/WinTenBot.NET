using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using WinTenBot.Providers;
using WinTenBot.Services;

namespace WinTenBot.Telegram
{
    public static class Metrics
    {
        public static async Task HitActivityAsync(this TelegramService telegramService)
        {
            Log.Information("Starting Hit Activity");

            var message = telegramService.MessageOrEdited;
            var botUser = await telegramService.GetMeAsync()
                .ConfigureAwait(false);
            var data = new Dictionary<string, object>()
            {
                {"via_bot", botUser.Username},
                {"message_type", message.Type.ToString()},
                {"from_id", message.From.Id},
                {"from_first_name", message.From.FirstName},
                {"from_last_name", message.From.LastName},
                {"from_username", message.From.Username},
                {"from_lang_code", message.From.LanguageCode},
                {"chat_id", message.Chat.Id},
                {"chat_username", message.Chat.Username},
                {"chat_type", message.Chat.Type.ToString()},
                {"chat_title", message.Chat.Title},
            };

            var insertHit = await new Query("hit_activity")
                .ExecForMysql(true)
                .InsertAsync(data)
                .ConfigureAwait(false);

            Log.Information($"Insert Hit: {insertHit}");
        }

        public static void HitActivityBackground(this TelegramService telegramService)
        {
            BackgroundJob.Enqueue(() => HitActivityAsync(telegramService));

            Log.Information("Hit Activity scheduled in Background");
        }
    }
}