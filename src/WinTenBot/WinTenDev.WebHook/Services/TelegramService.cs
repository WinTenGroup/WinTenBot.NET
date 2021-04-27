using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace WinTenDev.WebHook.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly TelegramBotClient _client;

        public TelegramService(IConfiguration configuration)
        {
            var token = configuration["BotToken"];

            _client = new TelegramBotClient(token);
        }

        public async Task SendMessage(long chatId, string message)
        {
            try
            {
                Log.Information("Sending message to {ChatId}", chatId);
                await _client.SendTextMessageAsync(
                    chatId,
                    message,
                    ParseMode.Html);

                Log.Information("Send finish");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error send to {ChatId}", chatId);
            }
        }
    }

    public interface ITelegramService
    {
        public Task SendMessage(long chatId, string message);
    }
}