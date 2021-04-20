using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;
using Zizi.Bot.Services.Features;

namespace Zizi.Bot.Telegram
{
    public static class GBanAdminUtil
    {
        private const string GBanAdminTable = "gban_admin";

        public static async Task<bool> IsGBanAdminAsync(this long userId)
        {
            var querySql = await new Query(GBanAdminTable)
                .ExecForMysql()
                .Where("user_id", userId)
                .GetAsync<GBanAdminItem>();
            var isRegistered = querySql.Any();
            Log.Debug("UserId {0} is registered on ES2? {1}", userId, isRegistered);

            
            return isRegistered;
        }
        private static async Task<bool> IsRegisteredGBanAsync(this GBanAdminItem gBanAdminItem)
        {
            return await IsGBanAdminAsync(gBanAdminItem.UserId);
        }

        public static async Task RegisterGBanAdmin(this TelegramService telegramService)
        {
            var message = telegramService.Message;
            var userId = message.From.Id;
            if (message.ReplyToMessage != null)
            {
                var repMsg = message.ReplyToMessage;
                if (repMsg.From.IsBot)
                {
                    await telegramService.EditAsync("Tidak dapat meregister Bot menjadi admin ES2");
                    return;
                }

                userId = message.ReplyToMessage.From.Id;
            }

            var adminItem = new GBanAdminItem()
            {
                Username = message.From.Username,
                UserId = userId,
                PromotedBy = message.From.Id,
                PromotedFrom = message.Chat.Id,
                CreatedAt = DateTime.Now,
                IsBanned = false,
            };

            var isRegistered = await adminItem.IsRegisteredGBanAsync();
            if (isRegistered)
            {
                await telegramService.EditAsync($"Sepertinya UserID {adminItem.UserId} sudah menjadi Admin Fed");
                return;
            }

            var querySql = await new Query(GBanAdminTable)
                .ExecForMysql()
                .InsertAsync(adminItem);
            Log.Debug("Insert GBanReg: {0}", querySql);

            await telegramService.EditAsync("Selesai");
        }
    }
}