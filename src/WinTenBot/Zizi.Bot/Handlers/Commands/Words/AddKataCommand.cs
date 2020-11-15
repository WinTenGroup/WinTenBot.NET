using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Telegram;
using Zizi.Bot.Services;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Words
{
    public class AddKataCommand : CommandBase
    {
        private TelegramService _telegramService;
        private WordFilterService _wordFilterService;

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args,
            CancellationToken cancellationToken)
        {
            _telegramService = new TelegramService(context);
            _wordFilterService = new WordFilterService(context.Update.Message);

            var msg = context.Update.Message;
            var cleanedMsg = msg.Text.GetTextWithoutCmd();
            var partedMsg = cleanedMsg.Split(" ");
            var paramOption = partedMsg.ValueOfIndex(1) ?? "";
            var word = partedMsg.ValueOfIndex(0);
            var isGlobalBlock = false;

            var isSudoer = _telegramService.IsSudoer();
            var isAdmin = await _telegramService.IsAdminGroup()
                .ConfigureAwait(false);
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
                    await _telegramService.AppendTextAsync("Kata ini akan di blokir Global!")
                        .ConfigureAwait(false);
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
                await _telegramService.AppendTextAsync("Hanya Sudoer yang dapat memblokir Kata mode Group-wide!")
                    .ConfigureAwait(false);
            }

            if (word.IsNotNullOrEmpty())
            {
                await _telegramService.AppendTextAsync("Sedang menambahkan kata")
                    .ConfigureAwait(false);

                var isExist = await _wordFilterService.IsExistAsync(@where)
                    .ConfigureAwait(false);
                if (!isExist)
                {
                    var save = await _wordFilterService.SaveWordAsync(word, isGlobalBlock)
                        .ConfigureAwait(false);

                    await _telegramService.AppendTextAsync("Sinkronisasi Kata ke cache")
                        .ConfigureAwait(false);
                    await Sync.SyncWordToLocalAsync()
                        .ConfigureAwait(false);

                    await _telegramService.AppendTextAsync("Kata berhasil di tambahkan")
                        .ConfigureAwait(false);
                }
                else
                {
                    await _telegramService.AppendTextAsync("Kata sudah di tambahkan")
                        .ConfigureAwait(false);
                }
            }
            else
            {
                await _telegramService.SendTextAsync("Apa kata yg mau di blok?")
                    .ConfigureAwait(false);
            }

            await _telegramService.DeleteAsync(delay: 3000)
                .ConfigureAwait(false);
        }
    }
}