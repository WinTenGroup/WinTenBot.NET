using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using WinTenBot.Common;
using WinTenBot.Services;
using WinTenBot.Telegram;
using WinTenBot.Tools;

namespace WinTenBot.Handlers.Commands.Core
{
    public class TestCommand : CommandBase
    {
        private RssService _rssService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            _rssService = new RssService(context.Update.Message);

            var chatId = _telegramService.Message.Chat.Id;
            var fromId = _telegramService.Message.From.Id;
            var msg = _telegramService.Message;
            var msgId = msg.MessageId;

            if (fromId.IsSudoer())
            {
                Log.Information("Test started..");
                await _telegramService.SendTextAsync("Sedang mengetes sesuatu")
                    .ConfigureAwait(false);


                msg.AddCache($"anu-{msgId}");
                msg.MessageId.AddCache($"anu");
                _telegramService.SetChatCache("settings", msg);

                var keys = MonkeyCacheUtil.GetKeys();
                Log.Debug("Keys: {0}", keys.ToJson(true));

                var data = MonkeyCacheUtil.Get<string>("anu");
                Log.Debug("Data: {0}", data.ToJson(true));


                // Telegram.Metrics.FlushHitActivity();

                // var data = await new Query("rss_history")
                //     .Where("chat_id", chatId)
                //     .ExecForMysql()
                //     .GetAsync();
                //
                // var rssHistories = data
                //     .ToJson()
                //     .MapObject<List<RssHistory>>();
                //
                // ConsoleHelper.WriteLine(data.GetType());
                // // ConsoleHelper.WriteLine(data.ToJson(true));
                //
                // ConsoleHelper.WriteLine(rssHistories.GetType());
                // // ConsoleHelper.WriteLine(rssHistories.ToJson(true));
                //
                // ConsoleHelper.WriteLine("Test completed..");

                // await "This test".LogToChannel();

                // await RssHelper.SyncRssHistoryToCloud();
                // await BotHelper.ClearLog();

                // await SyncHelper.SyncGBanToLocalAsync();
                // var greet = TimeHelper.GetTimeGreet();

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    // new[]
                    // {
                    // InlineKeyboardButton.WithCallbackData("Warn Username Limit", "info warn-username-limit"),
                    // InlineKeyboardButton.WithCallbackData("-", "callback-set warn_username_limit 3"),
                    // InlineKeyboardButton.WithCallbackData("4", "info setelah"),
                    // InlineKeyboardButton.WithCallbackData("+", "callback-set warn_username_limit 5")
                    // },
                    new[]
                    {
                        // InlineKeyboardButton.WithCallbackData("Warn Username Limit", "info warn-username-limit"),
                        InlineKeyboardButton.WithCallbackData("-", "callback-set warn_username_limit 3"),
                        InlineKeyboardButton.WithCallbackData("4", "info setelah"),
                        InlineKeyboardButton.WithCallbackData("+", "callback-set warn_username_limit 5")
                    }
                });

                // await _telegramService.EditAsync("Warn Username Limit", inlineKeyboard);

                // LearningHelper.Setup2();
                // LearningHelper.Predict();


                // if (msg.ReplyToMessage != null)
                // {
                //     var repMsg = msg.ReplyToMessage;
                //     var repMsgText = repMsg.Text;
                //
                //     Log.Information("Predicting message");
                //     var isSpam = MachineLearning.PredictMessage(repMsgText);
                //     await _telegramService.EditAsync($"IsSpam: {isSpam}")
                //         .ConfigureAwait(false);
                //
                //     return;
                // }

                await _telegramService.EditAsync("Complete")
                    .ConfigureAwait(false);
            }

            // else
            // {
            //     await _requestProvider.SendTextAsync("Unauthorized.");
            // }
        }
    }
}