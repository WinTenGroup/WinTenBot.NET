﻿using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenBot.Services;
using WinTenBot.Telegram;

namespace WinTenBot.Handlers.Commands.Tags
{
    public class UntagCommand : CommandBase
    {
        private TagsService _tagsService;
        private TelegramService _telegramService;

        public UntagCommand()
        {
            _tagsService = new TagsService();
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            var msg = _telegramService.Message;

            var isSudoer = _telegramService.IsSudoer();
            var isAdmin = await _telegramService.IsAdminOrPrivateChat();
            var tagVal = args[0];
            var chatId = _telegramService.Message.Chat.Id;
            var sendText = "Hanya admin yang bisa membuat Tag.";

            if (!isAdmin && !isSudoer)
            {
                await _telegramService.SendTextAsync(sendText);
                Log.Information("This User is not Admin or Sudo!");
                return;
            }

            await _telegramService.SendTextAsync("Memeriksa..");
            var isExist = await _tagsService.IsExist(chatId, tagVal);
            if (isExist)
            {
                Log.Information($"Sedang menghapus tag {tagVal}");
                var unTag = await _tagsService.DeleteTag(chatId, tagVal);
                if (unTag)
                {
                    sendText = $"Hapus tag {tagVal} berhasil";
                }

                await _telegramService.EditAsync(sendText);
                return;
            }
            else
            {
                sendText = $"Tag {tagVal} tidak di temukan";
            }
        }
    }
}