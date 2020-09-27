using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Notes
{
    public class NotesCommand : CommandBase
    {
        private NotesService _notesService;
        private TelegramService _telegramService;

        public NotesCommand()
        {
            _notesService = new NotesService();
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);

            await _telegramService.SendTextAsync("This feature currently disabled")
                .ConfigureAwait(false);
            return;
            
            var notesData = await _notesService.GetNotesByChatId(_telegramService.Message.Chat.Id)
                .ConfigureAwait(false);

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

            await _notesService.UpdateCache(_telegramService.Message.Chat.Id)
                .ConfigureAwait(false);

            await _telegramService.SendTextAsync(sendText)
                .ConfigureAwait(false);
        }
    }
}