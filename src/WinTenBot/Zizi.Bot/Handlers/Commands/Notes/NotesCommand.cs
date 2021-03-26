using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;

namespace Zizi.Bot.Handlers.Commands.Notes
{
    public class NotesCommand : CommandBase
    {
        private readonly NotesService _notesService;
        private readonly TelegramService _telegramService;

        public NotesCommand(NotesService notesService, TelegramService telegramService)
        {
            _notesService = notesService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            await _telegramService.SendTextAsync("This feature currently disabled");
            return;

            var notesData = await _notesService.GetNotesByChatId(_telegramService.Message.Chat.Id);

            var sendText = "Filters di Obrolan ini.";

            if (notesData.Rows.Count > 0)
            {
                foreach (DataRow note in notesData.Rows)
                {
                    sendText += $"\nID: {note["id_note"]} - ";
                    sendText += $"{note["slug"]}";
                }
            }
            else
            {
                sendText = "Tidak ada Filters di Grup ini." +
                           "\nUntuk menambahkannya ketik /addfilter";
            }

            await _notesService.UpdateCache(_telegramService.Message.Chat.Id);

            await _telegramService.SendTextAsync(sendText);
        }
    }
}