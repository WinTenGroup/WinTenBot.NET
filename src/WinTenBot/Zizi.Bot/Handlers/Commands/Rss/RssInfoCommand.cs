using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;
using Zizi.Core.Utils.Text;

namespace Zizi.Bot.Handlers.Commands.Rss
{
    public class RssInfoCommand : CommandBase
    {
        private readonly RssService _rssService;
        private readonly TelegramService _telegramService;

        public RssInfoCommand(RssService rssService, TelegramService telegramService)
        {
            _telegramService = telegramService;
            _rssService = rssService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var chatId = _telegramService.Message.Chat.Id;
            var isAdmin = await _telegramService.IsAdminGroup();

            if (!isAdmin && !_telegramService.IsPrivateChat)
            {
                await _telegramService.SendTextAsync("Kamu bukan admin, atau kamu bisa mengaturnya di japri.");
                return;
            }

            await _telegramService.SendTextAsync("🔄 Sedang meload data..");
            var rssData = await _rssService.GetRssSettingsAsync(chatId);
            var rssCount = rssData.Count;

            var sb = new StringBuilder();

            sb.AppendLine($"📚 <b>List RSS</b>: {rssCount} Items.");
            int num = 1;
            foreach (var rss in rssData)
            {
                sb.AppendLine($"{num++}. {rss.UrlFeed}");
            }

            if (rssCount == 0)
            {
                sb.AppendLine("\nSepertinya kamu belum menambahkan RSS disini. ");
                sb.AppendLine("Gunakan <code>/setrss https://link_rss_nya</code> untuk menambahkan.");
            }
            
            await _telegramService.EditAsync(sb.ToTrimmedString());
        }
    }
}