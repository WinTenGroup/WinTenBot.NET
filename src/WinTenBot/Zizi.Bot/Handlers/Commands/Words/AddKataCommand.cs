using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services.Datas;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Telegram;

namespace Zizi.Bot.Handlers.Commands.Words
{
    public class AddKataCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly WordFilterService _wordFilterService;

        public AddKataCommand(
            TelegramService telegramService,
            WordFilterService wordFilterService
        )
        {
            _telegramService = telegramService;
            _wordFilterService = wordFilterService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var chatId = _telegramService.ChatId;
            var fromId = _telegramService.FromId;
            var msg = context.Update.Message;
            var cleanedMsg = msg.Text.GetTextWithoutCmd();
            var partedMsg = cleanedMsg.Split(" ");
            var paramOption = partedMsg.ValueOfIndex(1) ?? "";
            var word = partedMsg.ValueOfIndex(0);
            var isGlobalBlock = false;

            var isSudoer = _telegramService.IsSudoer();
            var isAdmin = await _telegramService.IsAdminGroup();
            if (!isSudoer)
            {
                Log.Information("Currently add Kata is limited only Sudo.");
                return;
            }

            if (word.IsValidUrl())
            {
                word = word.ParseUrl().Path;
            }

            var where = new Dictionary<string, object>() {{"word", word}};

            if (paramOption.IsContains("-"))
            {
                if (paramOption.IsContains("g") && isSudoer) // Global
                {
                    isGlobalBlock = true;
                    await _telegramService.AppendTextAsync("Kata ini akan di blokir Global!");
                }

                if (paramOption.IsContains("d"))
                {
                }

                if (paramOption.IsContains("c"))
                {
                }
            }

            if (!paramOption.IsContains("g"))
            {
                @where.Add("chat_id", msg.Chat.Id);
            }

            if (!isSudoer)
            {
                await _telegramService.AppendTextAsync("Hanya Sudoer yang dapat memblokir Kata mode Group-wide!");
            }

            if (!word.IsNotNullOrEmpty())
            {
                await _telegramService.SendTextAsync("Apa kata yg mau di blok?");
            }
            else
            {
                await _telegramService.AppendTextAsync("Sedang menambahkan kata");

                var isExist = await _wordFilterService.IsExistAsync(@where);
                if (isExist)
                {
                    await _telegramService.AppendTextAsync("Kata sudah di tambahkan");
                }
                else
                {
                    var save = await _wordFilterService.SaveWordAsync(new WordFilter()
                    {
                        Word = word,
                        ChatId = chatId,
                        IsGlobal = isGlobalBlock,
                        FromId = fromId,
                        CreatedAt = DateTime.Now
                    });

                    await _telegramService.AppendTextAsync("Sinkronisasi Kata ke cache");
                    await _wordFilterService.UpdateWordsCache();
                    // await _queryFactory.SyncWordToLocalAsync();

                    await _telegramService.AppendTextAsync("Kata berhasil di tambahkan");
                }
            }

            await _telegramService.DeleteAsync(delay: 3000);
        }
    }
}