using Serilog;
using SqlKata.Execution;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Services;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers
{
    public class NewUpdateHandler : IUpdateHandler
    {
        private TelegramService _telegramService;
        private AppConfig AppConfig { get; set; }
        private QueryFactory QueryFactory { get; set; }

        public NewUpdateHandler(AppConfig appConfig, QueryFactory queryFactory)
        {
            AppConfig = appConfig;
            QueryFactory = queryFactory;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            if (_telegramService.Context.Update.ChannelPost != null) return;

            _telegramService.AppConfig = AppConfig;
            _telegramService.IsBotAdmin = await _telegramService.IsBotAdmin().ConfigureAwait(false);
            _telegramService.IsFromAdmin = await _telegramService.IsAdminChat().ConfigureAwait(false);

            var chatSettings = _telegramService.CurrentSetting;

            Log.Debug("NewUpdate MessageType: {0}", _telegramService.AnyMessage.Type);
            Log.Debug("NewUpdate: {0}", _telegramService.Context.Update.ToJson(true));

            // Pre-Task is should be awaited.
            await EnqueuePreTask().ConfigureAwait(false);

            if (chatSettings.EnableWarnUsername && _telegramService.IsGroupChat())
            {
                Log.Debug("Await next condition 1. is enable Warn Username && is Group Chat.");
                if (!_telegramService.IsNoUsername || _telegramService.MessageOrEdited.Text == null)
                {
                    // Next, do what bot should do.
                    Log.Debug("Await next condition on sub condition 1. is has Username || MessageText == null");
                    await next(context, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (_telegramService.IsPrivateChat())
            {
                Log.Debug("Await next condition 2. if private chat");
                await next(context, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Log.Debug("Await next else condition.");
                await next(context, cancellationToken).ConfigureAwait(false);
            }

            //
            // if (_telegramService.MessageOrEdited.Text == null)
            // {
            //     await next(context, cancellationToken).ConfigureAwait(false);
            // }

            // Last, do additional task which bot may do
            EnqueueBackgroundTask();
        }

        private async Task EnqueuePreTask()
        {
            Log.Information("Enqueue pre tasks");
            var sw = Stopwatch.StartNew();

            var message = _telegramService.MessageOrEdited;
            var callbackQuery = _telegramService.CallbackQuery;

            // var actions = new List<Action>();
            var shouldAwaitTasks = new List<Task>();

            if (!_telegramService.IsPrivateChat())
            {
                shouldAwaitTasks.Add(_telegramService.EnsureChatRestrictionAsync());

                if (message.Text != null)
                {
                    shouldAwaitTasks.Add(_telegramService.CheckGlobalBanAsync());
                    shouldAwaitTasks.Add(_telegramService.CheckCasBanAsync());
                    shouldAwaitTasks.Add(_telegramService.CheckSpamWatchAsync());
                    shouldAwaitTasks.Add(_telegramService.CheckUsernameAsync());
                }
            }

            if (callbackQuery == null)
            {
                shouldAwaitTasks.Add(_telegramService.CheckMessageAsync());
            }

            Log.Debug("Awaiting should await task..");

            await Task.WhenAll(shouldAwaitTasks.ToArray())
                .ConfigureAwait(false);

            Log.Debug("All preTask completed in {0}", sw.Elapsed);
            sw.Stop();
        }


        private void EnqueueBackgroundTask()
        {
            var nonAwaitTasks = new List<Task>();
            var message = _telegramService.MessageOrEdited;

            //Exec nonAwait Tasks
            Log.Debug("Running nonAwait task..");
            nonAwaitTasks.Add(_telegramService.EnsureChatHealthAsync());
            nonAwaitTasks.Add(_telegramService.AfkCheckAsync());

            if (message.Text != null)
            {
                // nonAwaitTasks.Add(_telegramService.FindNotesAsync());
                nonAwaitTasks.Add(_telegramService.FindTagsAsync());
            }

            nonAwaitTasks.Add(_telegramService.CheckMataZiziAsync());
            nonAwaitTasks.Add(_telegramService.HitActivityAsync());

#pragma warning disable 4014
            // This List Task should not await.
            Task.WhenAny(nonAwaitTasks.ToArray());
#pragma warning restore 4014
        }
    }
}