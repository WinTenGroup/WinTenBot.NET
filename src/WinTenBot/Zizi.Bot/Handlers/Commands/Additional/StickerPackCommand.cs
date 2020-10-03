using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Telegram;
using Zizi.Bot.Enums;
using Zizi.Bot.Models;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Additional
{
    public class StickerPackCommand : CommandBase
    {
        private TelegramService _telegramService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var client = _telegramService.Client;
            var message = _telegramService.Message;
            var chatId = message.Chat.Id;

            if (!await _telegramService.IsBeta().ConfigureAwait(false)) return;

            if (message.ReplyToMessage == null)
            {
                await _telegramService.SendTextAsync("Balas pesan Stiker untuk membangun StikerPack")
                    .ConfigureAwait(false);
                return;
            }

            var repMsg = message.ReplyToMessage;
            var repMsgId = repMsg.MessageId;

            if (repMsg.Type != MessageType.Sticker)
            {
                await _telegramService.SendTextAsync("Balas pesan Stiker untuk membangun StikerPack.")
                    .ConfigureAwait(false);
                return;
            }

            await _telegramService.SendTextAsync("Sedang mengumpulkan StikerSet..")
                .ConfigureAwait(false);

            var sticker = repMsg.Sticker;
            Log.Debug("Sticker: {0}", sticker.ToJson(true));

            var setName = sticker.SetName;
            var stickerPack = await _telegramService.Client.GetStickerSetAsync(setName, cancellationToken)
                .ConfigureAwait(false);
            Log.Debug("StikerPack: {0}", stickerPack.ToJson(true));

            var listStickers = stickerPack.Stickers;

            var sendEdit = $"Sedang mengunduh {listStickers.Length} Stiker" +
                           $"\nNama: {stickerPack.Name}" +
                           $"\nJudul: {stickerPack.Title}";

            await _telegramService.EditAsync(sendEdit).ConfigureAwait(false);
            var cachePath = Path.Combine(BotSettings.PathCache, chatId.ToString(), "stikerpack-" + repMsgId)
                .SanitizeSlash().EnsureDirectory(true);
            var packsPath = Path.Combine(cachePath, "stiker-packs").EnsureDirectory(true);
            // await cachePath.ClearLogs(".webp", upload: false).ConfigureAwait(false);

            foreach (var listSticker in listStickers)
            {
                var fileId = listSticker.FileId;
                var filePath = Path.Combine(packsPath, fileId + ".webp").SanitizeSlash();
                Log.Debug("Downloading Sticker: {0} to {1}", fileId, filePath);
                var fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
                await client.GetInfoAndDownloadFileAsync(fileId, fileStream, cancellationToken:
                    cancellationToken).ConfigureAwait(false);
                fileStream.Close();
                fileStream.Dispose();
            }

            await _telegramService.EditAsync("Sedang membangun StikerPack..").ConfigureAwait(false);
            var listStikerItem = new List<StickerItem>();
            foreach (var listSticker in listStickers)
            {
                var filePath = listSticker.FileId + ".webp";
                listStikerItem.Add(new StickerItem()
                {
                    Emojis = new List<string>() {listSticker.Emoji},
                    ImageFile = filePath
                });
            }

            var listStikerPacks = new List<StickerPack>
            {
                new StickerPack()
                {
                    Identifier = "stiker-packs",
                    Name = sticker.SetName,
                    Publisher = "ZiZi StikerPack Kit",
                    TrayImageFile = listStikerItem.First().ImageFile,
                    ImageDataVersion = 1,
                    AvoidCache = false,
                    PublisherEmail = "zizibot@azhe.space",
                    PublisherWebsite = new Uri("https://github.com/WinTenDev/WinTenBot.NET"),
                    PrivacyPolicyWebsite = "",
                    LicenseAgreementWebsite = "https://github.com/WinTenDev/WinTenBot.NET/blob/master/LICENSE",
                    Stickers = listStikerItem
                }
            };

            var stikerPacksJson = new StickerAppItem()
            {
                AndroidPlayStoreLink = new Uri("https://play.google.com/store/apps/details?id=com.kanelai.stickerapp"),
                IosAppStoreLink = "",
                StickerPacks = listStikerPacks
            }.ToJson(true);

            var contents = Path.Combine(cachePath, "contents.json");

            await _telegramService.EditAsync("Sedang menulis Metadata..").ConfigureAwait(false);
            await File.WriteAllTextAsync(contents, stikerPacksJson, cancellationToken).ConfigureAwait(false);

            await _telegramService.EditAsync("Sedang Membuat paket StikerPacks..").ConfigureAwait(false);
            var zipFileName = $"zizi-stikerpacks-{sticker.SetName}-{repMsgId}.stikerpacks";
            var packName = Path.Combine(cachePath, $"zizi-stikerpacks-{sticker.SetName}-{repMsgId}.stikerpacks");
            var files = Directory.GetFiles(cachePath, "*.*", SearchOption.AllDirectories)
                .Where(x => !x.Contains(".stikerpacks"));
            var zipPack = files.CreateZip(packName);
            // cachePath.CreateZip(packName);

            await _telegramService.SendMediaAsync(packName, MediaType.LocalDocument).ConfigureAwait(false);
        }
    }
}