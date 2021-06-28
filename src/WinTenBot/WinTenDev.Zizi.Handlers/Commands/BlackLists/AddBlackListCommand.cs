using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace WinTenDev.Zizi.Handlers.Commands.BlackLists
{
    public class AddBlackListCommand : CommandBase
    {
        public AddBlackListCommand()
        {
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
        }
    }
}