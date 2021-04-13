using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Scheduler;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Rss
{
    public class RssCtlCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly RssService _rssService;

        public RssCtlCommand(
            TelegramService telegramService,
            RssService rssService
        )
        {
            _rssService = rssService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var chatId = _telegramService.ChatId;

            var msg = context.Update.Message;

            var isSudoer = _telegramService.IsSudoer();
            if (!isSudoer)
            {
                Log.Warning("This command only for sudo!");
                return;
            }

            var partedMsg = msg.Text.Split(" ");
            var param1 = partedMsg.ValueOfIndex(1);
            Log.Debug("RssCtl Param1: {Param1}", param1);

            await _telegramService.AppendTextAsync("Access Granted");
            switch (param1)
            {
                case "start":
                    await _telegramService.AppendTextAsync("Starting RSS Service");
                    // RssScheduler.InitScheduler();

                    var rssSettings = await _rssService.GetAllRssSettingsAsync();
                    var filteredSettings = rssSettings.Where(setting => setting.ChatId == chatId.ToString());

                    foreach (var rssSetting in rssSettings)
                    {
                        var rssChatId = rssSetting.ChatId.ToInt64();
                        var urlFeed = rssSetting.UrlFeed;

                        var reducedChatId = rssChatId.ReduceChatId();
                        var unique = String.GenerateUniqueId(5);

                        var baseId = "rss";
                        var cronInMinute = 1;
                        var recurringId = $"{baseId}-{reducedChatId}-{unique}";

                        HangfireUtil.RegisterJob<RssFeedService>(recurringId, service => service.ExecuteUrlAsync(rssChatId, urlFeed), Cron.Minutely, queue: "rss-feed");

                        // RegisterFeed(chatId, urlFeed);
                        // RegisterScheduler(chatId);
                    }

                    await _telegramService.AppendTextAsync("Start successfully.");
                    break;

                case "stop":
                    await _telegramService.AppendTextAsync("Stopping RSS Service");
                    HangfireUtil.DeleteAllJobs();
                    await _telegramService.AppendTextAsync("Stop successfully.");
                    break;

                default:
                    await _telegramService.AppendTextAsync("Lalu mau ngapain?");
                    break;
            }
        }
    }
}