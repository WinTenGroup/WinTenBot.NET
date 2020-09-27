using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;

namespace Zizi.Bot.Handlers.Commands.Words
{
    public class DeleteKataCommand:CommandBase
    {
        public override Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}