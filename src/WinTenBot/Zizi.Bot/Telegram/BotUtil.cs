using System.Threading.Tasks;
using Serilog;
using Zizi.Bot.Services;

namespace Zizi.Bot.Telegram
{
    public static class BotUtil
    {
        public static async Task<bool> IsBotAdmin(this TelegramService telegramService)
        {
            var message = telegramService.MessageOrEdited;
            var chat = message.Chat;

            var me = await telegramService.GetMeAsync().ConfigureAwait(false);
            var isBotAdmin = await telegramService.IsAdminGroup(me.Id).ConfigureAwait(false);
            Log.Debug("Is {0} Admin on Chat {1}? {2}", me.Username, chat.Id, isBotAdmin);

            return isBotAdmin;
        }
    }
}