using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Host.Telegram;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Rss
{
    public class RssPullCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public RssPullCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var chatId = _telegramService.Message.Chat.Id;
            var isAdmin = await _telegramService.IsAdminGroup();

            if (!isAdmin && !_telegramService.IsPrivateChat)
            {
                Log.Warning("You must Admin or Private chat");

                return;
            }

            Log.Information("Pulling RSS in {0}", chatId);

#pragma warning disable 4014
            Task.Run(async () =>
#pragma warning restore 4014
            {
                await _telegramService.SendTextAsync("Sedang memeriksa RSS feed baru..");

                var reducedChatId = chatId.ReduceChatId();
                var recurringId = $"rss-{reducedChatId}";
                var count = HangfireUtil.TriggerJobs(recurringId);

                if (count == 0)
                {
                    await _telegramService.EditAsync("Tampaknya tidak ada RSS baru saat ini");
                }
                else
                {
                    await _telegramService.DeleteAsync(_telegramService.SentMessageId);
                }

            }, cancellationToken);
        }
    }
}