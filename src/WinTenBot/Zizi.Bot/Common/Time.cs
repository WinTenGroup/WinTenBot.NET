using System;
using Serilog;

namespace Zizi.Bot.Common
{
    public static class Time
    {
        public static string GetDelay(this DateTime time)
        {
            var date1 = DateTime.Now.ToUniversalTime();
            var date2 = time;
            // Console.WriteLine($"Date1: {date1}, Date2: {date2}");

            var timeSpan = (date1 - date2);

            return timeSpan.ToString(@"s\,fff");
        }

        public static string GetTimeGreet()
        {
            var greet = "dini hari";
            var hour = DateTime.Now.Hour;

            if (hour <= 3) greet = "dini hari";
            else if (hour <= 11) greet = "pagi";
            else if (hour <= 14) greet = "siang";
            else if (hour <= 17) greet = "sore";
            else if (hour <= 18) greet = "petang";
            else if (hour <= 24) greet = "malam";

            Log.Information("Current hour: {Hour}, greet: {Greet}", hour, greet);

            return greet;
        }

        public static long GetMuteStep(this long step)
        {
            var muteResult = step switch
            {
                1 => 10,
                2 => 30,
                3 => 8 * 60,
                4 => 365 * 24 * 60,
                _ => -1
            };

            return muteResult;
        }
    }
}