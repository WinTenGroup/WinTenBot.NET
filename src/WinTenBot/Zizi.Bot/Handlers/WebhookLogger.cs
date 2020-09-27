using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;

namespace Zizi.Bot.Handlers
{
    class WebhookLogger : IUpdateHandler
    {
        // private readonly ILogger<WebhookLogger> _logger;

        // public WebhookLogger(
        //     ILogger<WebhookLogger> logger
        // )
        // {
        //     _logger = logger;
        // }

        public Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            var httpContext = (HttpContext)context.Items[nameof(HttpContext)];
            var updateId = context.Update.Id;
            var httpHost = httpContext.Request.Host;
            
            $"Received update {updateId} in a webhook at {httpHost}".LogInfo();
            
            // _logger.LogInformation(
            //     "Received update {0} in a webhook at {1}.",
            //     context.Update.Id,
            //     httpContext.Request.Host
            // );

            return next(context, cancellationToken);
        }
    }
}
