using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Models;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Services.HangfireJobs
{
    public class ChatService
    {
        private readonly QueryFactory _queryFactory;
        private TelegramBotClient _botClient;

        public ChatService(
            QueryFactory queryFactory,
            TelegramBotClient botClient
        )
        {
            _botClient = botClient;
            _queryFactory = queryFactory;
        }

        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task CheckBotAdminOnGroup()
        {
            Log.Information("Starting Check bot is Admin on all Group!");
            var botClient = BotSettings.Client;
            var urlAddTo = await botClient.GetUrlStart("startgroup=new");

            var chatGroups = _queryFactory.FromQuery(new Query("group_settings"))
                .WhereNot("chat_type", "Private")
                .WhereNot("chat_type", "0")
                .Get<ChatSetting>();

            foreach (var chatGroup in chatGroups)
            {
                var chatId = chatGroup.ChatId;
                var isAdmin = chatGroup.IsAdmin;
                Log.Debug("Bot is Admin on {0}? {1}", chatId, isAdmin);

                // var me = await botClient.GetMeAsync();
                // var isAdminChat = await _botClient.IsAdminChat(long.Parse(chatId), me.Id);

                // if (isAdminChat) continue;

                Log.Debug("Doing leave chat from {0}", chatId);
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

                    await botClient.SendTextMessageAsync(chatId, msgLeave, ParseMode.Html, replyMarkup: inlineKeyboard);
                    await botClient.LeaveChatAsync(chatId);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("bot is not a member"))
                    {
                        Log.Warning("This bot may has leave from this chatId '{0}'", chatId);
                    }
                    else if (ex.Message.ToLower().Contains("forbidden"))
                    {
                        Log.Warning(ex.Message);
                    }
                    else
                    {
                        Log.Error(ex.Demystify(), "Error on Leaving from ChatID: {0}", chatId);
                    }
                }
            }
        }
    }
}