using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;

namespace WinTenDev.Zizi.Host.Handlers
{
    class UpdateMembersList : IUpdateHandler
    {
        public Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log.Information("Updating chat members list...");
            Console.ResetColor();

            return next(context);
        }
    }
}
