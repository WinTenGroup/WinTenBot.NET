using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;
using Zizi.Core.Utils;

namespace Zizi.Bot.Telegram
{
    public static class Metrics
    {
        public static async Task HitActivityAsync(this TelegramService telegramService)
        {
            Log.Information("Starting Hit Activity");

            var message = telegramService.MessageOrEdited;
            var botUser = await telegramService.GetBotMeAsync();

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

            // var hitActivity = new HitActivity()
            // {
            //     ViaBot = botUser.Username,
            //     MessageType = message.Type.ToString(),
            //     FromId = message.From.Id,
            //     FromFirstName = message.From.FirstName,
            //     FromLastName = message.From.LastName,
            //     FromUsername = message.From.Username,
            //     FromLangCode = message.From.LanguageCode,
            //     ChatId = message.Chat.Id.ToString(),
            //     ChatUsername = message.Chat.Username,
            //     ChatType = message.Chat.Type.ToString(),
            //     ChatTitle = message.Chat.Title,
            //     Timestamp = DateTime.Now
            // };

            // Log.Debug("Inserting to LiteDB.");
            // var metrics = LiteDbProvider.GetCollections<HitActivity>();
            // metrics.Insert(hitActivity);
            // Log.Debug("Buffer saved.");
        }

        public static void HitActivityBackground(this TelegramService telegramService)
        {
            BackgroundJob.Enqueue(() => HitActivityAsync(telegramService));

            Log.Information("Hit Activity scheduled in Background");
        }

        public static void FlushHitActivity()
        {
            Log.Information("Flushing HitActivity buffer");
            var metrics = LiteDbProvider.GetCollections<HitActivity>();

            var dateFormat = "yyyy-MM-dd HH";
            var dateFormatted = DateTime.Now.ToString(dateFormat);
            Log.Debug("Filter last hour: {0}", dateFormatted);
            var filteredMetrics = metrics.Find(x =>
                x.Timestamp.ToString(dateFormat) == dateFormatted).ToList();

            if (filteredMetrics.Count == 0)
            {
                Log.Debug("No HitActivity buffed need to flush.");
                return;
            }

            Log.Debug("Flushing {0} of {1} data..", filteredMetrics.Count, metrics.Count());
            foreach (var m in filteredMetrics)
            {
                var data = new Dictionary<string, object>()
                {
                    {"via_bot", m.ViaBot},
                    {"message_type", m.MessageType},
                    {"from_id", m.FromId},
                    {"from_first_name", m.FromFirstName},
                    {"from_last_name", m.FromLastName},
                    {"from_username", m.FromUsername},
                    {"from_lang_code", m.FromLangCode},
                    {"chat_id", m.ChatId},
                    {"chat_username", m.ChatUsername},
                    {"chat_type", m.ChatType},
                    {"chat_title", m.ChatTitle},
                    {"timestamp", m.Timestamp}
                };

                var insertHit = new Query("hit_activity")
                    .ExecForMysql(true)
                    .Insert(data);

                // Log.Information($"Insert Hit: {insertHit}");
            }

            Log.Debug("Clearing local data..");
            filteredMetrics.ForEach(x => { metrics.DeleteMany(y => y.Timestamp == x.Timestamp); });

            LiteDbProvider.Rebuild();

            Log.Information("Flush HitActivity done.");
        }

        public static async Task GetStat(this TelegramService telegramService)
        {
            var chatId = telegramService.Message.Chat.Id;
            var statBuilder = new StringBuilder();
            var monthStr = DateTime.Now.ToString("yyyy-MM");
            statBuilder.AppendLine($"Stat Group: {chatId}");

            await telegramService.SendTextAsync(statBuilder.ToString().Trim())
                .ConfigureAwait(false);

            var monthCount = await GetMonthlyStat(telegramService)
                .ConfigureAwait(false);
            var monthRates = monthCount / 30;
            statBuilder.AppendLine($"This Month: {monthCount}");
            statBuilder.AppendLine($"Traffics: {monthRates} msg/day");
            await telegramService.EditAsync(statBuilder.ToString().Trim())
                .ConfigureAwait(false);

            statBuilder.AppendLine();

            var todayCount = await GetDailyStat(telegramService)
                .ConfigureAwait(false);
            var todayRates = todayCount / 24;
            statBuilder.AppendLine($"Today: {todayCount}");
            statBuilder.AppendLine($"Traffics: {todayRates} msg/hour");
            await telegramService.EditAsync(statBuilder.ToString().Trim())
                .ConfigureAwait(false);
        }

        private static async Task<int> GetMonthlyStat(this TelegramService telegramService)
        {
            var chatId = telegramService.Message.Chat.Id;
            var statBuilder = new StringBuilder();
            var monthStr = DateTime.Now.ToString("yyyy-MM");
            // statBuilder.AppendLine($"Stat Group: {chatId}");

            var monthActivity = (await new Query("hit_activity")
                .ExecForMysql(true)
                .WhereRaw($"str_to_date(timestamp,'%Y-%m-%d') like '{monthStr}%'")
                .Where("chat_id", chatId)
                .GetAsync()
                .ConfigureAwait(false)).ToList();
            var monthCount = monthActivity.Count;
            return monthCount;
            // statBuilder.AppendLine($"This Month: {monthCount}");
            // await telegramService.EditAsync(statBuilder.ToString())
            //     .ConfigureAwait(false);
        }

        private static async Task<int> GetDailyStat(this TelegramService telegramService)
        {
            var chatId = telegramService.Message.Chat.Id;
            // var statBuilder = new StringBuilder();
            var todayStr = DateTime.Today.ToString("yyyy-MM-dd");

            var todayActivity = (await new Query("hit_activity")
                .ExecForMysql(true)
                .WhereDate("timestamp", todayStr)
                .Where("chat_id", chatId)
                .GetAsync()
                .ConfigureAwait(false)).ToList();

            var todayCount = todayActivity.Count;
            return todayCount;

            // statBuilder.AppendLine($"Today: {todayCount}");
            // await telegramService.EditAsync(statBuilder.ToString())
            //     .ConfigureAwait(false);
        }
    }
}