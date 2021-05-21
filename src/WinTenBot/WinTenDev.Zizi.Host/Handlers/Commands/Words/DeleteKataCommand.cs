using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Words
{
    public class DeleteKataCommand:CommandBase
    {
        public override Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}