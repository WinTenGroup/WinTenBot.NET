﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Helpers;
using WinTenBot.Providers;
using WinTenBot.Services;

namespace WinTenBot.Handlers.Commands.Tags
{
    public class TagCommand : CommandBase
    {
        private TagsService _tagsService;
        private TelegramProvider _telegramProvider;

        public TagCommand()
        {
            _tagsService = new TagsService();
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            var msg = context.Update.Message;
            _telegramProvider = new TelegramProvider(context);
            var isSudoer = msg.From.Id.IsSudoer();

            var sendText = "ℹ Simpan tag ke Cloud Tags" +
                           "\nContoh: <code>/tag tagnya [tombol|link.tombol]</code> - Reply pesan" +
                           "\nPanjang tag minimal 3 karakter." +
                           "\nTanda [ ] artinya tidak harus" +
                           "\n" +
                           "\n📝 <i>Jika untuk grup, di rekomendasikan membuat sebuah channel, " +
                           "lalu link ke post di tautkan sebagai tombol.</i>";

            if (!isSudoer)
            {
                sendText = "This feature currently limited";
            }

            if (msg.ReplyToMessage != null && isSudoer)
            {
                Log.Information("Replied message detected..");
                Log.Information($"Arg0: {args[0]}");

                if (args[0].Length >= 3)
                {
                    await _telegramProvider.SendTextAsync("📖 Mengumpulkan informasi..");
//                    Log.Information(TextHelper.ToJson(msg.ReplyToMessage));
                    var repMsg = msg.ReplyToMessage;

                    var content = repMsg.Text ?? repMsg.Caption;
                    Log.Information(content);

                    bool isExist = await _tagsService.IsExist(msg.Chat.Id, args[0].Trim());
                    Log.Information($"Tag isExist: {isExist}");
                    if (!isExist)
                    {
                        var data = new Dictionary<string, object>()
                        {
                            {"id_chat", msg.Chat.Id},
                            {"id_user", msg.From.Id},
                            {"tag", args[0].Trim()},
                            {"content", content}
                        };

                        await _telegramProvider.EditAsync("📝 Menyimpan tag data..");
                        await _tagsService.SaveTag(data);

//                        var keyboard = new InlineKeyboardMarkup(
//                            InlineKeyboardButton.WithCallbackData("OK", "tag finish-create")
//                        );

                        await _telegramProvider.EditAsync("✅ Tag berhasil di simpan..");

                        await _tagsService.UpdateCacheAsync(msg);
                        return;
                    }

                    await _telegramProvider.EditAsync(
                        "✅ Tag sudah ada. Silakan ganti Tag jika ingin isi konten berbeda");
                }

                await _telegramProvider.EditAsync("Slug Tag minimal 3 karakter");
            }
            else
            {
                foreach (var arg in args)
                {
                    Console.WriteLine(arg);
                }

                await _telegramProvider.SendTextAsync(sendText);
            }
        }
    }
}