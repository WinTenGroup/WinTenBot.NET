using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Serilog;

namespace WinTenDev.ZiziBot.Utils.Parsers.Url
{
    public static class DirectLinkUtil
    {
        private static readonly IConfiguration Config = Configuration.Default.WithDefaultLoader().WithJs().WithCss();

        public static async Task<string> ParseZippyShare(this string url)
        {
            var sw = Stopwatch.StartNew();

            Log.Information("Preparing parse Zippyshare.com");
            var context = BrowsingContext.New(Config);

            Log.Debug("Loading web {Url}", url);
            var document = await context.OpenAsync(url);

            Log.Debug("Finding download button..");
            var dl = document.QuerySelectorAll<IHtmlAnchorElement>("a")
                .FirstOrDefault(x => x.Id == "dlbutton");

            if (dl == null) return string.Empty;

            Log.Debug("Getting direct link..");
            var directLink = dl.Href;

            Log.Information("Directlink of {Url} => {DirectLink}", url, directLink);

            return directLink;
        }
    }
}