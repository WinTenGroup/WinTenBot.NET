using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Services.Features;
using Zizi.Bot.Tools;

namespace Zizi.Bot.Handlers.Commands.Additional
{
    public class TranslateCommand : CommandBase
    {
        private readonly TelegramService _telegramService;

        public TranslateCommand(TelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var message = _telegramService.Message;
            var userLang = message.From.LanguageCode;

            if (message.ReplyToMessage == null)
            {
                var hint = await "Balas pesan yang ingin anda terjemahkan".GoogleTranslatorAsync(userLang);
                await _telegramService.SendTextAsync(hint);

                return;
            }

            var param = message.Text.SplitText(" ").ToArray();
            var param1 = param.ValueOfIndex(1) ?? "";

            if (param1.IsNullOrEmpty())
            {
                param1 = message.From.LanguageCode;
            }

            var forTranslate = message.ReplyToMessage.Text;

            Log.Information("Param: {0}", param.ToJson(true));

            await _telegramService.SendTextAsync("🔄 Translating into Your language..");
            try
            {
                var translate = await forTranslate.GoogleTranslatorAsync(param1);

                // var translate = await forTranslate.TranslateAsync(param1);

                // var translate = forTranslate.TranslateTo(param1);

                // var translateResult = new StringBuilder();
                // foreach (var translation in translate.Result.Translations)
                // {
                // translateResult.AppendLine(translation._Translation);
                // }

                // var translateResult = translate.MergedTranslation;

                await _telegramService.EditAsync(translate);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Demystify(), "Error translation");

                var messageError = "Error translation" +
                                   $"\nMessage: {ex.Message}";
                await _telegramService.EditAsync(messageError);
            }
        }
    }
}