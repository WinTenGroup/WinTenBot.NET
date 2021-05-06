using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;

namespace WinTenDev.Mirror.Handlers
{
    public class ExceptionHandler : IUpdateHandler
    {
        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var u = context.Update;

            try
            {
                await next(context, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log.Error(exception.Demystify(), "Exception Handler");
                Log.Error("An error occured in handling update {0}", u.Id);
            }
        }
    }
}