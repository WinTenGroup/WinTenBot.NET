using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Exceptionless;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Utils.Text;

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
                Log.Debug("Current Update: {U}", u.ToJson());

                e.ToExceptionless().Submit();
            }
        }
    }
}