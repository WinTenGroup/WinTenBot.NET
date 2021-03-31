using System.Threading.Tasks;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Services;
using Zizi.Bot.Services.Datas;

namespace Zizi.Bot.Telegram
{
    public static class Group
    {
        // public static async Task SaveWelcome(this TelegramService telegramService, string target)
        // {
        //     var msg = telegramService.Message;
        //     var chatId = msg.Chat.Id;
        //     var columnTarget = $"welcome_{target}";
        //     var data = msg.Text.GetTextWithoutCmd();
        //     var settingsService = new SettingsService(msg);
        //
        //     if (data.IsNullOrEmpty())
        //     {
        //         await telegramService.SendTextAsync($"Silakan masukan konfigurasi {target} yang akan di terapkan");
        //         return;
        //     }
        //     // Log.Information(columnTarget);
        //     // Log.Information(data);
        //
        //     await telegramService.SendTextAsync($"Sedang menyimpan Welcome {target}..");
        //
        //     await settingsService.UpdateCell(chatId, columnTarget, data);
        //
        //     await telegramService.EditAsync($"Welcome {target} berhasil di simpan!" +
        //                                     $"\nKetik /welcome untuk melihat perubahan");
        //
        //     Log.Information("Success save welcome {Target} on {ChatId}.", target, chatId);
        // }
    }
}