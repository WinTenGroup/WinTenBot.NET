using System.Threading.Tasks;
using GoogleTranslateFreeApi;

namespace Zizi.Bot.Providers
{
    public static class GoogleTranslateProvider
    {

        public static async Task<TranslationResult> TranslateAsync(this string forTranslate, string toLang)
        {
            var translator = new GoogleTranslator();
            var from = Language.Auto;
            var to = GoogleTranslator.GetLanguageByISO(toLang);

            var result = await translator.TranslateLiteAsync(forTranslate, from, to)
                .ConfigureAwait(false);

            return result;
        }
    }
}