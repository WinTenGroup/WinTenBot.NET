using System.IO;
using System.Threading.Tasks;

namespace WinTenDev.Zizi.Utils
{
    public static class DocsUtil
    {
        public static async Task<string> LoadInBotDocs(this string slug)
        {
            var path = $"Storage/InbotDocs/{slug}.html";
            var html = await File.ReadAllTextAsync(path);

            return html.Trim();
        }
    }
}