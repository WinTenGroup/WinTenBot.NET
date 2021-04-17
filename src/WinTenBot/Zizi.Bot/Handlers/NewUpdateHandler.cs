using Humanizer;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers
{
    public class NewUpdateHandler : IUpdateHandler
    {
        private readonly TelegramService _telegramService;
        private readonly AfkService _afkService;
        private readonly AntiSpamService _antiSpamService;
        private readonly SettingsService _settingsService;

        private ChatSetting _chatSettings;
        private readonly AppConfig _appConfig;

        public NewUpdateHandler(
            AfkService afkService,
            AppConfig appConfig,
            AntiSpamService antiSpamService,
            SettingsService settingsService,
            TelegramService telegramService
        )
        {
            _appConfig = appConfig;
            _afkService = afkService;
            _antiSpamService = antiSpamService;
            _telegramService = telegramService;
            _settingsService = settingsService;
        }

        public async Task HandleAsync(IUpdateContext context, UpdateDelegate next, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            if (_telegramService.Context.Update.ChannelPost != null) return;

            _telegramService.IsTooOld();

            _chatSettings = _telegramService.CurrentSetting;

            Log.Debug("NewUpdate: {0}", _telegramService.ToJson(true));

            // Pre-Task is should be awaited.
            await EnqueuePreTask();

            if (_chatSettings.EnableWarnUsername
                && _telegramService.IsGroupChat())
            {
                Log.Debug("Await next condition 1. is enable Warn Username && is Group Chat.");
                if (!_telegramService.IsNoUsername || _telegramService.MessageOrEdited.Text == null)
                {
                    // Next, do what bot should do.
                    Log.Debug("Await next condition on sub condition 1. is has Username || MessageText == null");
                    await next(context, cancellationToken);
                }
            }
            else if (_telegramService.IsPrivateChat)
            {
                Log.Debug("Await next condition 2. if private chat");
                await next(context, cancellationToken);
            }
            else
            {
                Log.Debug("Await next else condition.");
                await next(context, cancellationToken);
            }

            //
            // if (_telegramService.MessageOrEdited.Text == null)
            // {
            //     await next(context, cancellationToken);
            // }

            // Last, do additional task which bot may do
            await EnqueueBackgroundTask();
        }

        private async Task EnqueuePreTask()
        {
            Log.Information("Enqueue pre tasks");
            var sw = Stopwatch.StartNew();

            var message = _telegramService.AnyMessage;
            var callbackQuery = _telegramService.CallbackQuery;

            var shouldAwaitTasks = new List<Task>();

            if (!_telegramService.IsPrivateChat)
            {
                shouldAwaitTasks.Add(_telegramService.EnsureChatRestrictionAsync());

                if (message?.Text != null)
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

            await Task.WhenAll(shouldAwaitTasks.ToArray());

            Log.Debug("All preTask completed in {0}", sw.Elapsed);
            sw.Stop();
        }

        private async Task EnqueueBackgroundTask()
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
                // nonAwaitTasks.Add(_telegramService.FindTagsAsync());
            }

            nonAwaitTasks.Add(_telegramService.CheckMataZiziAsync());
            nonAwaitTasks.Add(_telegramService.HitActivityAsync());

            // This List Task should not await.
            await Task.WhenAll(nonAwaitTasks.ToArray());
        }
        }
    }
}