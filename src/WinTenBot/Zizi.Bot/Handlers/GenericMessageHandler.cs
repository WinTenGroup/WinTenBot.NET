using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;

namespace Zizi.Bot.Handlers
{
    public class GenericMessageHandler : IUpdateHandler
    {
        // private CasBanProvider _casBanProvider;
        // private ChatProcessor _chatProcessor;
        // private NotesService _notesService;
        // private IReplyMarkup _replyMarkup;
        // private AfkService _afkService;
        // private ElasticSecurityService _elasticSecurityService;

        public GenericMessageHandler()
        {
            // _casBanProvider = new CasBanProvider();
            // _notesService = new NotesService();
            // _afkService = new AfkService();
            // _replyMarkup = null;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            // _chatProcessor = new ChatProcessor(context);
            // _elasticSecurityService = new ElasticSecurityService(context.Update.Message);
            
            // Message msg = context.Update.Message;

            // ConsoleHelper.WriteLine(msg.ToJson());
            // ConsoleHelper.WriteLine(msg.Text);


            // if (msg.Text == "ping")
            // {
                // run /ping?
                
            // }
            
            // if (Bot.HostingEnvironment.IsProduction())
            // {
            //     await _casBanProvider.IsCasBan(msg.From.Id);
            // }

            // Parallel.Invoke(async () => await CheckMessage(msg));
            
            // var selectedNotes = await _notesService.GetNotesBySlug(msg.Chat.Id, msg.Text);
            // if (selectedNotes.Rows.Count > 0)
            // {
            //     var content = selectedNotes.Rows[0]["content"].ToString();
            //     var btnData = selectedNotes.Rows[0]["btn_data"].ToString();
            //     if (btnData != "")
            //     {
            //         _replyMarkup = btnData.ToReplyMarkup(2);
            //     }
            //     
            //     await _chatProcessor.SendAsync(content,_replyMarkup);
            //     _replyMarkup = null;
            //
            //     foreach (var note in selectedNotes.Rows)
            //     {
            //         ConsoleHelper.WriteLine(note.ToJson());
            //     }
            // }
            // else
            // {
            //     ConsoleHelper.WriteLine("No rows result set in Notes");
            // }

            //            await context.Bot.Client.SendTextMessageAsync(
            //                msg.Chat, "You said:\n" + msg.Text
            //            );

            // await AfkCheck(msg);
            // await CheckGlobalBanAsync(msg);
            // CheckUsername(msg);

            await next(context, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}