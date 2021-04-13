using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Rss
{
    public class DelRssCommand : CommandBase
    {
        private readonly RssService _rssService;
        private readonly TelegramService _telegramService;

        public DelRssCommand(RssService rssService, TelegramService telegramService)
        {
            _rssService = rssService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);
            var chatId = _telegramService.ChatId;

            var isAdminOrPrivateChat = _telegramService.IsAdminOrPrivateChat();
            if (!isAdminOrPrivateChat)
            {
                Log.Warning("Delete RSS only for admin or private chat!");
                await _telegramService.DeleteAsync();
                return;
            }

            var urlFeed = _telegramService.Message.Text.GetTextWithoutCmd();

            await _telegramService.SendTextAsync($"Sedang menghapus {urlFeed}");

            var delete = await _rssService.DeleteRssAsync(chatId, urlFeed);

            var success = delete.ToBool()
                ? "berhasil."
                : "gagal. Mungkin RSS tersebut sudah di hapus atau belum di tambahkan";

            await _telegramService.EditAsync($"Hapus {urlFeed} {success}");
        }
    }
}