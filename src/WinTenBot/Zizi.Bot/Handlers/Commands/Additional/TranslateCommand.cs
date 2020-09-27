using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using Zizi.Bot.Common;
using Zizi.Bot.Providers;
using Zizi.Bot.Services;

namespace Zizi.Bot.Handlers.Commands.Additional
{
    public class TranslateCommand:CommandBase
    {
        private TelegramService _telegramService;
        
        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        { 
            _telegramService = new TelegramService(context);

            var message = _telegramService.Message;
            var userLang = message.From.LanguageCode;

            if (message.ReplyToMessage != null)
            {
                var param = message.Text.SplitText(" ").ToArray();
                var param1 = param.ValueOfIndex(1) ?? "";

                // if (param1.IsNullOrEmpty() ||  !param1.Contains("-"))
                // {
                //     await _telegramProvider.SendTextAsync("Lang code di perlukan. Contoh: en-id, English ke Indonesia");
                //     return;
                // }

                if (param1.IsNullOrEmpty())
                {
                    param1 = message.From.LanguageCode;
                }
                
                var forTranslate = message.ReplyToMessage.Text;

                Log.Information($"Param: {param.ToJson(true)}");

                await _telegramService.SendTextAsync("🔄 Translating into Your language..")
                    .ConfigureAwait(false);

                var translate = await forTranslate.TranslateAsync(param1)
                    .ConfigureAwait(false);

                // var translate = forTranslate.TranslateTo(param1);
                
                // var translateResult = new StringBuilder();
                // foreach (var translation in translate.Result.Translations)
                // {
                    // translateResult.AppendLine(translation._Translation);
                // }

                var translateResult = translate.MergedTranslation;

                await _telegramService.EditAsync(translateResult)
                    .ConfigureAwait(false);
            }
            else
            {
                var hint = await "Balas pesan yang ingin anda terjemahkan".TranslateAsync(userLang)
                    .ConfigureAwait(false);
                await _telegramService.SendTextAsync(hint.MergedTranslation)
                    .ConfigureAwait(false);
            }

        }
    }
}