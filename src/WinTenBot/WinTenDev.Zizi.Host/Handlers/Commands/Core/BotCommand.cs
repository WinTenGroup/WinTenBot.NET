using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Host.Telegram;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Core
{
    public class BotCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public BotCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var isSudoer = _telegramService.IsSudoer();
            if (!isSudoer) return;

            // var param1 = _telegramService.Message.Text.Split(" ").ValueOfIndex(1);
            // switch (param1)
            // {
            //     case "migrate":
            //         await _telegramService.SendTextAsync("Migrating ")
            //             ;
            //         SqlMigration.MigrateMysql();
            //         // SqlMigration.MigrateSqlite();
            //         await _telegramService.SendTextAsync("Migrate complete ")
            //             ;
            //
            //         break;
            // }
        }
    }
}