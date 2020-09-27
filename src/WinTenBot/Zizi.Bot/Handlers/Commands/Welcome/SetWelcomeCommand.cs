using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Welcome
{
    public class SetWelcomeCommand : CommandBase
    {
        private SettingsService _settingsService;
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = context.Update.Message;
            _settingsService = new SettingsService(msg);

            if (msg.Chat.Type == ChatType.Private)
            {
                await _telegramService.SendTextAsync("Welcome hanya untuk grup saja")
                    .ConfigureAwait(false);
                return;
            }
            
            await _telegramService.SendTextAsync("/setwelcome sudah di pisah menjadi /welmsg, /welbtn dan /weldoc")
                .ConfigureAwait(false);
            return;

            var partsMsg = msg.Text.Split(' ').ToArray();

            string[] commands = {"message", "msg", "button", "btn"};
            Log.Information(partsMsg.ToJson());

            var isAdmin = await _telegramService.IsAdminGroup();
            if (isAdmin)
            {
                if (msg.ReplyToMessage != null)
                {
                    var repMsg = msg.ReplyToMessage;
                    if (repMsg.GetFileId() != "")
                    {
                        var mediaFileId = repMsg.GetFileId();
                        var mediaType = repMsg.Type;

                        await _telegramService.SendTextAsync("Sedang menyimpan Welcome Media..");
                        Log.Information($"MediaId: {mediaFileId}");

                        await _settingsService.UpdateCell("welcome_media", mediaFileId);
                        await _settingsService.UpdateCell("welcome_media_type", mediaType);
                        Log.Information("Save media success..");

                        await _telegramService.EditAsync("Welcome Media berhasil di simpan.");
                        return;
                    }
                    else
                    {
                        await _telegramService.SendTextAsync("Media tidak terdeteksi di pesan yg di reply tersebut.");
                        return;
                    }
                }

                var missParamText = $"Parameter yg di dukung {string.Join(", ", commands)}" +
                                    $"\nContoh: <code>/setwelcome message</code>";

                if (partsMsg.Length > 1)
                {
                    if (commands.Contains(partsMsg[1]))
                    {
                        if (partsMsg[2] != null)
                        {
                            var target = partsMsg[1]
                                .Replace("msg", "message")
                                .Replace("btn", "button");
                            var columnTarget = $"welcome_{target}";
                            var data = msg.Text
                                .Replace(partsMsg[0], "")
                                .Replace(partsMsg[1], "").Trim();

                            // Log.Information(columnTarget);
                            // Log.Information(data);

                            await _telegramService.SendTextAsync("Sedang menyimpan Welcome Message..");

                            await _settingsService.UpdateCell(columnTarget, data);
                            await _telegramService.EditAsync($"Welcome {target} berhasil di simpan!");
                        }
                        else
                        {
                            await _telegramService.SendTextAsync("Masukan Pesan atau tombol yang akan di tetapkan");
                        }
                    }
                    else
                    {
                        await _telegramService.SendTextAsync(missParamText);
                    }
                }
                else
                {
                    await _telegramService.SendTextAsync(missParamText);
                }
            }
        }
    }
}