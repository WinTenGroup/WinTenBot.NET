using System.IO;
using System.Threading.Tasks;

namespace Zizi.Bot.Common
{
    public static class Docs
    {
        public static async Task<string> LoadInBotDocs(this string slug)
        {
            var path = $"Storage/InbotDocs/{slug}.html";
            var html = await File.ReadAllTextAsync(path)
                .ConfigureAwait(false);

            return html.Trim();
        }
    }
}