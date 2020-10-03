using Hangfire;
using Zizi.Bot.Models;

namespace Zizi.Bot.Scheduler
{
    public static class DigestScheduler
    {
        public static void SendMessage()
        {
            RecurringJob.AddOrUpdate(() => SendGroot(), Cron.Minutely);
        }

        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public static void SendGroot()
        {
            BotSettings.Client.SendTextMessageAsync("-1001404591750", "I'm Groot");
        }
    }
}