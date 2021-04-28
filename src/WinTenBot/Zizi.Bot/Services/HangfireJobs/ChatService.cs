using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using SqlKata.Execution;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Models;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Telegram;
using Zizi.Core.Utils;

namespace Zizi.Bot.Services.HangfireJobs
{
    public class ChatService
    {
        private readonly QueryFactory _queryFactory;
        private readonly TelegramBotClient _botClient;
        private readonly QueryService _queryService;

        public ChatService(
            QueryFactory queryFactory,
            TelegramBotClient botClient,
            QueryService queryService
        )
        {
            _botClient = botClient;
            _queryFactory = queryFactory;
            _queryService = queryService;
        }

        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task CheckBotAdminOnGroup()
        {
            Log.Information("Starting Check bot is Admin on all Group!");
            // var urlAddTo = await _botClient.GetUrlStart("startgroup=new");

            var queryFactory = _queryService.CreateMySqlConnection();
            var chatGroups = queryFactory.FromTable("group_settings")
                .WhereNot("chat_type", "Private")
                .WhereNot("chat_type", "0")
                .Get<ChatSetting>();

            foreach (var chatGroup in chatGroups)
            {
                var chatId = chatGroup.ChatId;
                // var isAdmin = chatGroup.IsAdmin;
                // Log.Debug("Bot is Admin on {0}? {1}", chatId, isAdmin);

                var me = await _botClient.GetMeAsync();
                var isAdminChat = await _botClient.IsAdminChat(long.Parse(chatId), me.Id);

                if (isAdminChat) continue;

                Log.Debug("Doing leave chat from {ChatId}", chatId);
                try
                {
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
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("bot is not a member"))
                    {
                        Log.Warning("This bot may has leave from this chatId '{ChatId}'", chatId);
                    }
                    else if (ex.Message.ToLower().Contains("forbidden"))
                    {
                        Log.Warning("{Message}", ex.Message);
                    }
                    else
                    {
                        Log.Error(ex.Demystify(), "Error on Leaving from ChatID: {ChatId}", chatId);
                    }
                }
            }
        }
    }
}