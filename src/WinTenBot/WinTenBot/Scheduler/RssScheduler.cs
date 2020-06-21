using System.Threading.Tasks;
using Hangfire;
using Serilog;
using WinTenBot.Common;
using WinTenBot.Services;
using WinTenBot.Telegram;
using WinTenBot.Tools;

namespace WinTenBot.Scheduler
{
    public static class RssScheduler
    {
        public static void InitScheduler()
        {
            Task.Run(async () =>
            {
                Log.Information("Initializing RSS Scheduler.");
                var rssService = new RssService();
                Log.Information("Getting list Chat ID");
                var listChatId = await rssService.GetListChatIdAsync()
                    .ConfigureAwait(false);
                foreach (var rssSetting in listChatId)
                {
                    var chatId = rssSetting.ChatId.ToInt64();
                    RegisterScheduler(chatId);
                }

                Log.Information("Registering RSS Scheduler complete.");
            });
        }

        public static void RegisterScheduler(this long chatId)
        {
            Log.Information("Initializing RSS Scheduler.");

            var baseId = "rss";
            var cronInMinute = 5;
            var reduceChatId = chatId.ToInt64().ReduceChatId();
            var recurringId = $"{baseId}-{reduceChatId}";

            Log.Information($"Creating Jobs for {chatId}");

            RecurringJob.RemoveIfExists(recurringId);
            RecurringJob.AddOrUpdate(recurringId, ()
                => RssBroadcaster.ExecBroadcasterAsync(chatId), $"*/{cronInMinute} * * * *");
            RecurringJob.Trigger(recurringId);

            Log.Information($"Registering RSS Scheduler for {chatId} complete.");
        }
    }
}