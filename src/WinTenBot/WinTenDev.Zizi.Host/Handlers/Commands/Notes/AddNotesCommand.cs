using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Telegram;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Notes
{
    public class AddNotesCommand : CommandBase
    {
        private readonly NotesService _notesService;
        private readonly TelegramService _telegramService;

        public AddNotesCommand(TelegramService telegramService, NotesService notesService)
        {
            _notesService = notesService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var msg = context.Update.Message;

            await _telegramService.SendTextAsync("This feature currently disabled");
            return;

            if (msg.ReplyToMessage != null)
            {
                var repMsg = msg.ReplyToMessage;
                await _telegramService.SendTextAsync("Mengumpulkan informasi");

                var partsContent = repMsg.Text.Split(new[] { "\n\n" }, StringSplitOptions.None);
                var partsMsgText = msg.Text.GetTextWithoutCmd().Split("\n\n");

                // Log.Information(msg.Text);
                // Log.Information(repMsg.Text);
                // Log.Information(partsContent.ToJson());
                // Log.Information(partsMsgText.ToJson());

                var data = new Dictionary<string, object>()
                {
                    {"slug", partsMsgText[0]},
                    {"content", partsContent[0]},
                    {"chat_id", msg.Chat.Id},
                    {"user_id", msg.From.Id}
                };

                if (!partsMsgText.ValueOfIndex(1).IsNullOrEmpty())
                {
                    data.Add("btn_data", partsMsgText[1]);
                }

                await _telegramService.EditAsync("Menyimpan..");
                await _notesService.SaveNote(data);

                await _telegramService.EditAsync("Berhasil");
            }
        }
    }
}