using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using SqlKata.Execution;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Telegram;

namespace WinTenDev.Zizi.Services
{
    public class ChatService
    {
        private string _adminCheckerPrefix = "admin-checker";

        private readonly QueryFactory _queryFactory;
        private readonly TelegramBotClient _botClient;
        private readonly QueryService _queryService;
        private readonly SettingsService _settingsService;

        public ChatService(
            QueryFactory queryFactory,
            TelegramBotClient botClient,
            QueryService queryService,
            SettingsService settingsService
        )
        {
            _botClient = botClient;
            _queryFactory = queryFactory;
            _queryService = queryService;
            _settingsService = settingsService;
        }

        public async Task RegisterChatHealth()
        {
            Log.Information("Starting Check bot is Admin on all Group!");

            // var urlAddTo = await _botClient.GetUrlStart("startgroup=new");

            var allSettings = _settingsService.GetAllSettings();
            foreach (var chatGroup in allSettings)
            {
                var chatId = chatGroup.ChatId.ToInt64();
                var reducedChatId = chatId.ReduceChatId();

                var adminCheckerId = _adminCheckerPrefix + "-" + reducedChatId;

                HangfireUtil.RegisterJob<ChatService>(adminCheckerId, (ChatService service) => service.AdminCheckerJobAsync(chatId), Cron.Daily, queue: "admin-checker");
            }
        }


        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task ChatCleanUp()
        {
            Log.Information("Starting Check bot is Admin on all Group!");
            // var urlAddTo = await _botClient.GetUrlStart("startgroup=new");

            // var queryFactory = _queryService.CreateMySqlConnection();
            // var chatGroups = queryFactory.FromTable("group_settings")
            // .WhereNot("chat_type", "Private")
            // .WhereNot("chat_type", "0")
            // .Get<ChatSetting>();

            var allSettings = _settingsService.GetAllSettings();

            foreach (var chatGroup in allSettings)
            {
                var chatId = chatGroup.ChatId.ToInt64();
                // var isAdmin = chatGroup.IsAdmin;
                // Log.Debug("Bot is Admin on {0}? {1}", chatId, isAdmin);

                try
                {
                    await AdminCheckerJobAsync(chatId);
                }
                catch (Exception ex)
                {
                    var msgEx = ex.Message.ToLower();

                    if (msgEx.Contains("bot is not a member"))
                    {
                        Log.Warning("This bot may has leave from this chatId '{ChatId}'", chatId);
                    }
                    // else if (msgEx.Contains("forbidden"))
                    // {
                    //     Log.Warning("{Message}", ex.Message);
                    // }
                    // else if (msgEx.Contains("not found"))
                    // {
                    //     Log.Warning("{Message}", ex.Message);
                    // }
                    else
                    {
                        Log.Error(ex.Demystify(), "Error when checking ChatID: {ChatId}", chatId);
                    }
                }
            }
        }

        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task AdminCheckerJobAsync(long chatId)
        {
            var sw = Stopwatch.StartNew();
            Log.Information("Starting check Admin in ChatID '{ChatId}'", chatId);

            var me = await _botClient.GetMeAsync();
            var isAdminChat = await _botClient.IsAdminChat(chatId, me.Id);

            if (isAdminChat) return;

            Log.Debug("Doing leave chat from {ChatId}", chatId);
            var msgLeave = "Sepertinya saya bukan admin di grup ini, saya akan meninggalkan grup. Sampai jumpa!" +
                           "\n\nTerima kasih sudah menggunakan @MissZiziBot, silakan undang saya kembali jika diperlukan.";

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("👥 Dukungan Grup", "https://t.me/WinTenDev")
                    // InlineKeyboardButton.WithUrl("↖️ Tambahkan ke Grup", urlAddTo)
                }
            });

            await _botClient.SendTextMessageAsync(chatId, msgLeave, ParseMode.Html, replyMarkup: inlineKeyboard);
            await _botClient.LeaveChatAsync(chatId);

            Log.Debug("Checking Admin in ChatID '{ChatId}' job complete in {Elapsed}", chatId, sw.Elapsed);
            sw.Stop();
        }
    }
}