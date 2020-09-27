using System;
using Serilog;

namespace Zizi.Bot.Tools
{
    [Obsolete("Please use Serilog directly.")]
    public static class ConsoleHelper
    {
        private static object BuildMsg(object message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
            return $"{timestamp} - {message}";
        }

        public static void ToConsoleStamp(this object obj)
        {
            WriteLine(obj);
        }
        
        public static void WriteLine(object message)
        {
            if (message == null) return;
            
            Log.Information(message.ToString());
            
            // var forConsole = BuildMsg(message);
            // Console.WriteLine(forConsole);
        }

        public static void Write(object message)
        {
            if (message == null) return;
            var forConsole = BuildMsg(message); 
            Console.WriteLine(forConsole);
        }
    }
}