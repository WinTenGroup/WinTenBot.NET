using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Zizi.Hook.Services
{
    public class TelegramService : ITelegramService
    {
        public TelegramService(IConfiguration configuration)
        {
            var token = configuration["BotToken"];

            Client = new TelegramBotClient(token);
        }

        private TelegramBotClient Client
        {
            get;
        }

        public async Task SendMessage(long chatId, string message)
        {
            try
            {
                Log.Information("Sending message to {0}", chatId);
                await Client.SendTextMessageAsync(
                    chatId,
                    message,
                    ParseMode.Html);

                Log.Information("Send finish");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error send to {0}", chatId);
            }
        }
    }

    public interface ITelegramService
    {
        public Task SendMessage(long chatId, string message);
    }
}