using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Zizi.Bot.Models;

namespace Zizi.Bot.Services.HangfireJobs
{
    public class ChatService
    {
        private readonly QueryFactory _queryFactory;

        public ChatService(QueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task CheckBotAdminOnGroup()
        {
            Log.Information("Starting Check bot is Admin on all Group!");
            var botClient = BotSettings.Client;
            var chatGroups = _queryFactory.FromQuery(new Query("group_settings"))
                .WhereNot("chat_type", "Private")
                .Get<ChatSetting>();

            foreach (var chatGroup in chatGroups)
            {
                var chatId = chatGroup.ChatId;
                var isAdmin = chatGroup.IsAdmin;
                Log.Debug("Bot is Admin on {0}? {1}", chatId, isAdmin);

                if (isAdmin) continue;

                Log.Debug("Doing leave chat from {0}", chatId);
                try
                {
                    await botClient.LeaveChatAsync(chatId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Demystify(), "Error on Leaving from ChatID: {0}", chatId);
                }
            }
        }
    }
}