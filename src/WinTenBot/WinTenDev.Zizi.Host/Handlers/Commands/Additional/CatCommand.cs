using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using File = System.IO.File;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Additional
{
    /// <summary>
    /// Get Kochenk (cat) single or many by param.<br/>
    /// Ex: <c>/cat</c> or <c>/cat 5</c>
    /// </summary>
    public class CatCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private const string CatSource = "https://aws.random.cat/meow";

        /// <summary>
        /// CatCommand constructor
        /// </summary>
        /// <param name="telegramService"></param>
        public CatCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        /// <summary>
        /// Handle CatCommand
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        public override async Task HandleAsync(
            IUpdateContext context,
            UpdateDelegate next,
            string[] args,
            CancellationToken cancellationToken
        )
        {
            await _telegramService.AddUpdateContext(context);

            var message = _telegramService.Message;
            var partsText = message.Text.SplitText(" ").ToArray();
            var catNum = 1;
            var param1 = partsText.ValueOfIndex(1);

            if (param1.IsNotNullOrEmpty())
            {
                if (!param1.IsNumeric())
                {
                    await _telegramService.SendTextAsync("Pastikan jumlah kochenk yang diminta berupa angka.");

                    return;
                }

                catNum = param1.ToInt();

                if (catNum > 10)
                {
                    await _telegramService.SendTextAsync("Berdasarkan Bot API, Batas maksimal Kochenk yg dapat di minta adalah 10.");

                    return;
                }
            }

            PrepareKochenk(catNum).FireAndForget();
        }

        private async Task PrepareKochenk(int catNum)
        {
            var listAlbum = new List<IAlbumInputMedia>();

            await _telegramService.SendTextAsync($"Sedang mempersiapkan {catNum} Kochenk");

            for (var i = 1; i <= catNum; i++)
            {
                Log.Information("Loading cat {I} of {CatNum} from {CatSource}", i, catNum, CatSource);

                var url = await CatSource.GetJsonAsync<CatMeow>();
                var urlFile = url.File.AbsoluteUri;

                Log.Debug("Adding kochenk {UrlFile}", urlFile);

                var fileName = Path.GetFileName(urlFile);
                var timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var uniqueId = StringUtil.GenerateUniqueId(5);
                var saveName = Path.Combine("Cats", $"kochenk_{timeStamp}_{uniqueId}_{fileName}");
                var savedPath = urlFile.SaveToCache(saveName);

                var fileStream = File.OpenRead(savedPath);

                var inputMediaPhoto = new InputMediaPhoto(new InputMedia(fileStream, fileName))
                {
                    Caption = $"Kochenk {i}",
                    ParseMode = ParseMode.Html
                };
                listAlbum.Add(inputMediaPhoto);

                await fileStream.DisposeAsync();
                Thread.Sleep(100);
            }

            await _telegramService.DeleteAsync();
            await _telegramService.SendMediaGroupAsync(listAlbum);
        }
    }
}