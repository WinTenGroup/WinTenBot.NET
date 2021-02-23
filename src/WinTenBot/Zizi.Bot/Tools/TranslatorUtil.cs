using System.Text;
using Flurl;
using Flurl.Http;
using Serilog;
using System.Threading.Tasks;
using Zizi.Bot.Models;

namespace Zizi.Bot.Tools
{
    /// <summary>
    /// The translator util.
    /// </summary>
    public static class TranslatorUtil
    {
        /// <summary>
        /// Translate text using Google Translate Clients5 (core).
        /// </summary>
        /// <param name="text">text is string for translate to</param>
        /// <param name="tl">tl is translation language</param>
        /// <param name="sl">sl is source language</param>
        /// <returns>A Task.</returns>
        public static async Task<GoogleClients5Translator> GoogleTranslatorCoreAsync(this string text, string tl, string sl = "auto")
        {
            var url = "https://clients5.google.com/translate_a/t";

            Log.Debug("Translating text from '{0} to '{1}'", sl, tl);
            var res = await url.SetQueryParam("sl", sl)
                .SetQueryParam("tl", tl)
                .SetQueryParam("client", "dict-chrome-ex")
                .SetQueryParam("q", text)
                .GetJsonAsync<GoogleClients5Translator>();

            return res;
        }

        public static async Task<string> GoogleTranslatorAsync(this string text, string tl, string sl = "auto")
        {
            var translate = await text.GoogleTranslatorCoreAsync(tl, sl);
            var sb = new StringBuilder();

            foreach (var sentence in translate.Sentences)
            {
                sb.AppendLine(sentence.Trans);
            }

            return sb.ToString().Trim();
        }
    }
}