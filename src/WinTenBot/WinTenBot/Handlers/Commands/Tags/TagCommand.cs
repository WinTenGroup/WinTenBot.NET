using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Common;
using WinTenBot.Services;
using WinTenBot.Telegram;

namespace WinTenBot.Handlers.Commands.Tags
{
    public class TagCommand : CommandBase
    {
        private TagsService _tagsService;
        private TelegramService _telegramService;

        public TagCommand()
        {
            _tagsService = new TagsService();
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;
            var isSudoer = _telegramService.IsSudoer();
            var isAdmin = await _telegramService.IsAdminOrPrivateChat()
                .ConfigureAwait(false);
            var sendText = "Hanya admin yang bisa membuat Tag";

            if (!isSudoer && !isAdmin)
            {
                // await _telegramService.DeleteAsync(msg.MessageId);
                await _telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
                Log.Information("This User is not Admin or Sudo!");
                return;
            }

            sendText = "ℹ Simpan tag ke Cloud Tags" +
                       "\nContoh: <code>/tag tagnya [tombol|link.tombol]</code> - Reply pesan" +
                       "\nPanjang tag minimal 3 karakter." +
                       "\nTanda [ ] artinya tidak harus" +
                       "\n" +
                       "\n📝 <i>Jika untuk grup, di rekomendasikan membuat sebuah channel, " +
                       "lalu link ke post di tautkan sebagai tombol.</i>";

            if (msg.ReplyToMessage != null)
            {
                Log.Information("Replied message detected..");

                var msgText = msg.Text;

                var repMsg = msg.ReplyToMessage;
                var repFileId = repMsg.GetFileId();
                var repMsgText = repMsg.Text;
                var partsMsgText = msgText.SplitText(" ").ToArray();

                Log.Information($"Part1: {partsMsgText.ToJson(true)}");

                var slugTag = partsMsgText.ValueOfIndex(1);
                var tagAndCmd = partsMsgText.Take(2).ToArray();
                var buttonData = msgText.RemoveThisString(tagAndCmd);

                if (slugTag.Length >= 3)
                {
                    await _telegramService.SendTextAsync("📖 Sedang mempersiapkan..")
                        .ConfigureAwait(false);

                    var content = repMsg.Text ?? repMsg.Caption ?? "";
                    Log.Information(content);

                    bool isExist = await _tagsService.IsExist(msg.Chat.Id, args[0].Trim())
                        .ConfigureAwait(false);
                    Log.Information($"Tag isExist: {isExist}");
                    if (!isExist)
                    {
                        var data = new Dictionary<string, object>()
                        {
                            {"chat_id", msg.Chat.Id},
                            {"from_id", msg.From.Id},
                            {"tag", slugTag.Trim()},
                            {"btn_data", buttonData},
                            {"content", content}
                        };

                        if (repFileId.IsNotNullOrEmpty())
                        {
                            data.Add("file_id", repFileId);
                            data.Add("type_data", repMsg.Type);
                        }

                        await _telegramService.EditAsync("📝 Menyimpan tag data..")
                            .ConfigureAwait(false);
                        await _tagsService.SaveTagAsync(data)
                            .ConfigureAwait(false);

                        // var keyboard = new InlineKeyboardMarkup(
                        //     InlineKeyboardButton.WithCallbackData("OK", "tag finish-create")
                        // );

                        await _telegramService.EditAsync("✅ Tag berhasil di simpan.." +
                                                         $"\nTag: <code>#{slugTag}</code>" +
                                                         $"\n\nKetik /tags untuk melihat semua Tag.")
                            .ConfigureAwait(false);

                        await _tagsService.UpdateCacheAsync(msg)
                            .ConfigureAwait(false);
                        return;
                    }

                    await _telegramService.EditAsync("✅ Tag sudah ada. " +
                                                     "Silakan ganti Tag jika ingin isi konten berbeda")
                        .ConfigureAwait(false);
                    return;
                }

                await _telegramService.EditAsync("Slug Tag minimal 3 karakter")
                    .ConfigureAwait(false);
            }
            else
            {
                await _telegramService.SendTextAsync(sendText)
                    .ConfigureAwait(false);
            }
        }
    }
}