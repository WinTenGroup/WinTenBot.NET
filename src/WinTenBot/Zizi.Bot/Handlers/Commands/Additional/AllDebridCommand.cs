using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;
using Zizi.Bot.Common;
using Zizi.Bot.Models.Settings;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Additional
{
    public class AllDebridCommand : CommandBase
    {
        private TelegramService _telegramService;
        private readonly AppConfig _appConfig;

        public AllDebridCommand(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context)
            {
                AppConfig = _appConfig
            };

            var allDebrid = _appConfig.AllDebridConfig;
            var isEnabled = allDebrid.IsEnabled;

            if (!isEnabled)
            {
                Log.Warning("AllDebrid feature is disabled on AppSettings");
                return;
            }

            var txtParts = _telegramService.MessageTextParts;
            var urlParam = txtParts.ValueOfIndex(1);

            if (!_telegramService.IsFromSudo && _telegramService.IsChatRestricted)
            {
                Log.Information("AllDebrid is restricted only to some Chat ID");
                var limitFeature = "Convert link via AllDebrid hanya boleh di grup <b>WinTen Mirror</b>.";
                var groupBtn = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("⬇Ke WinTen Mirror", "https://t.me/WinTenMirror")
                    }
                });

                await _telegramService.SendTextAsync(limitFeature, groupBtn)
                    .ConfigureAwait(false);

                return;
            }

            if (urlParam == null)
            {
                await _telegramService.SendTextAsync("Sertakan url yang akan di Debrid")
                    .ConfigureAwait(false);
                return;
            }

            Log.Information("Converting url: {0}", urlParam);
            await _telegramService.SendTextAsync("Sedang mengkonversi URL via Alldebrid.")
                .ConfigureAwait(false);

            var result = await _telegramService.ConvertUrl(urlParam).ConfigureAwait(false);
            if (result.Status != "success")
            {
                var errorMessage = result.DebridError.Message;
                var fail = "Sepertinya Debrid gagal." +
                           $"\nNote: {errorMessage}";

                await _telegramService.EditAsync(fail).ConfigureAwait(false);
                return;
            }

            var urlResult = result.DebridData.Link.AbsoluteUri;
            var fileName = result.DebridData.Filename;
            var fileSize = result.DebridData.Filesize;

            var text = "✅ Debrid berhasil" +
                       $"\n📁 Nama: <code>{fileName}</code>" +
                       $"\n📦 Ukuran: <code>{fileSize.SizeFormat()}</code>";

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("⬇️ Download", urlResult)
                }
            });

            await _telegramService.EditAsync(text, inlineKeyboard).ConfigureAwait(false);
        }
    }
}