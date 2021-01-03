using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;
using Telegram.Bot.Framework.Abstractions;

namespace Zizi.Bot.Handlers
{
    class WebhookLogger : IUpdateHandler
    {
        public Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var httpContext = (HttpContext) context.Items[nameof(HttpContext)];
            var updateId = context.Update.Id;
            var httpHost = httpContext.Request.Host;

            Log.Information("Received update {0} in a webhook at {1}", updateId, httpHost);

            return next(context, cancellationToken);
        }
    }
}