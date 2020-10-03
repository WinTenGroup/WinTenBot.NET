using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Models;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Rss
{
    public class RssInfoCommand : CommandBase
    {
        private RssService _rssService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            _rssService = new RssService(context.Update.Message);

            var chatId = _telegramService.Message.Chat.Id;
            var isAdmin = await _telegramService.IsAdminGroup();

            if (isAdmin || _telegramService.IsPrivateChat())
            {
                await _telegramService.SendTextAsync("🔄 Sedang meload data..");
                var rssData = await _rssService.GetRssSettingsAsync(chatId);
                var rssCount = rssData.Count();

                var sendText = $"📚 <b>List RSS</b>: {rssCount} Items.";
                int num = 1;
                foreach (RssSetting rss in rssData)
                {
                    // var urlFeed = rss["url_feed"].ToString();
                    sendText += $"\n{num++}. {rss.UrlFeed}";
                }

                if (rssCount == 0)
                {
                    sendText += "\n\nSepertinya kamu belum menambahkan RSS disini. " +
                                "Kamu dapat menambahkan RSS dengan jumlah tidak terbatas!" +
                                "\nGunakan <code>/setrss https://link_rss_nya</code> untuk menambahkan.";
                }

                await _telegramService.EditAsync(sendText);
            }
            else
            {
                await _telegramService.SendTextAsync("Kamu bukan admin, atau kamu bisa mengaturnya di japri ");
            }
        }
    }
}