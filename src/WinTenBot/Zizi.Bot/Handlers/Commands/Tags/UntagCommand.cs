using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Telegram;
using Zizi.Core.Utils;
using Zizi.Core.Utils.Text;

namespace Zizi.Bot.Handlers.Commands.Tags
{
    public class UntagCommand : CommandBase
    {
        private readonly TagsService _tagsService;
        private readonly TelegramService _telegramService;

        public UntagCommand(TagsService tagsService, TelegramService telegramService)
        {
            _tagsService = tagsService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = _telegramService.Message;

            var isSudoer = _telegramService.IsSudoer();
            var isAdmin = _telegramService.IsAdminOrPrivateChat();
            var tagVal = _telegramService.MessageTextParts.ValueOfIndex(1);
            var chatId = _telegramService.Message.Chat.Id;
            var sendText = "Hanya admin yang bisa membuat Tag.";

            if (!isAdmin && !isSudoer)
            {
                await _telegramService.SendTextAsync(sendText);
                Log.Information("This User is not Admin or Sudo!");
                return;
            }

            if (tagVal.IsNullOrEmpty())
            {
                await _telegramService.SendTextAsync("Tag apa yg mau di hapus?");
                return;
            }

            await _telegramService.SendTextAsync("Memeriksa..");
            var isExist = await _tagsService.IsExist(chatId, tagVal);
            if (isExist)
            {
                Log.Information("Sedang menghapus tag {TagVal}", tagVal);
                var unTag = await _tagsService.DeleteTag(chatId, tagVal);
                if (unTag)
                {
                    sendText = $"Hapus tag {tagVal} berhasil";
                }

                await _telegramService.EditAsync(sendText);
                await _tagsService.UpdateCacheAsync(chatId);
            }
            else
            {
                await _telegramService.EditAsync( $"Tag {tagVal} tidak di temukan");
            }
        }
    }
}