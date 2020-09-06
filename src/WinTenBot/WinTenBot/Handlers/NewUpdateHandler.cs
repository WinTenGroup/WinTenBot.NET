using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Common;
using WinTenBot.Services;
using WinTenBot.Telegram;

namespace WinTenBot.Handlers
{
    public class NewUpdateHandler : IUpdateHandler
    {
        private TelegramService _telegramService;

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            if (_telegramService.Context.Update.ChannelPost != null) return;

            var chatSettings = _telegramService.CurrentSetting;

            var update = _telegramService.Context.Update;

            $"NewUpdate: {update.ToJson(true)}".LogDebug();

            // Pre-Task is should be awaited.
            await EnqueuePreTask().ConfigureAwait(false);

            if (chatSettings.EnableWarnUsername)
            {
                if (!_telegramService.IsNoUsername)
                {
                    // Next, do what bot should do.
                    await next(context, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await next(context, cancellationToken).ConfigureAwait(false);
            }

            // Last, do additional task which bot may do
            EnqueueBackgroundTask();
        }

        private async Task EnqueuePreTask()
        {
            Log.Information("Enqueue pre tasks");

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

            Log.Information("Awaiting should await task..");

            await Task.WhenAll(shouldAwaitTasks.ToArray())
                .ConfigureAwait(false);
        }

        private void EnqueueBackgroundTask()
        {
            var nonAwaitTasks = new List<Task>();
            var message = _telegramService.MessageOrEdited;

            //Exec nonAwait Tasks
            Log.Information("Running nonAwait task..");
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