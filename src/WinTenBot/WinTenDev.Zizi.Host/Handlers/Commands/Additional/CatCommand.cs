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
    public class CatCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public CatCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var client = _telegramService.Client;
            var message = _telegramService.Message;
            var partsText = message.Text.SplitText(" ").ToArray();
            var chatId = message.Chat.Id;
            var listAlbum = new List<IAlbumInputMedia>();
            var catSource = "http://aws.random.cat/meow";
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
                    await _telegramService.SendTextAsync("Batas maksimal Kochenk yg di minta adalah 10");

                    return;
                }
            }

            await _telegramService.SendTextAsync($"Sedang mempersiapkan {catNum} Kochenk");

            for (int i = 1; i <= catNum; i++)
            {
                Log.Information("Loading cat {I} of {CatNum} from {CatSource}", i, catNum, catSource);

                var url = await catSource.GetJsonAsync<CatMeow>(cancellationToken);
                var urlFile = url.File.AbsoluteUri;

                Log.Information("Adding kochenk {UrlFile}", urlFile);

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

                // await fileStream.DisposeAsync();

                Thread.Sleep(100);
            }

            await _telegramService.DeleteAsync();
            await client.SendMediaGroupAsync(listAlbum, chatId, cancellationToken: cancellationToken);
        }
    }
}