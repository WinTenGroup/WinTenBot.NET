using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Exceptionless;
using Sentry;
using Serilog;
using Telegram.Bot.Framework.Abstractions;

namespace WinTenDev.Zizi.Host.Handlers
{
    public class ExceptionHandler : IUpdateHandler
    {
        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var u = context.Update;

            try
            {
                await next(context, cancellationToken);
            }
            catch (Exception e)
            {
                Log.Error(e.Demystify(), "Exception Handler");
                Log.Error("An error occured in handling update {Id}", u.Id);

                SentrySdk.CaptureException(e);
                e.ToExceptionless().Submit();
            }
        }
    }
}