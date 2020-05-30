using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Services;
using WinTenBot.Telegram;
using WinTenBot.Text;
using WinTenBot.Tools;

namespace WinTenBot.Handlers.Commands.Core
{
    public class BotCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var isSudoer = _telegramService.IsSudoer();
            if (!isSudoer) return;

            var param1 = _telegramService.Message.Text.Split(" ").ValueOfIndex(1);
            switch (param1)
            {
                case "migrate":
                    await _telegramService.SendTextAsync("Migrating ");
                    MigrationHelper.MigrateMysql();
                    MigrationHelper.MigrateSqlite();
                    await _telegramService.SendTextAsync("Migrate complete ");

                    break;
            }
        }
    }
}