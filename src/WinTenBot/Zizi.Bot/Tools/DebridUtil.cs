using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Zizi.Bot.Models;
using Zizi.Bot.Services.Features;

namespace Zizi.Bot.Tools
{
    public static class DebridUtil
    {
        private const string BaseUrl = "https://api.alldebrid.com/v4/";

        public static async Task<AllDebrid> ConvertUrl(this TelegramService telegramService, string url)
        {
            var appConfig = telegramService.AppConfig;
            var allDebrid = appConfig.AllDebridConfig;
            var agent = allDebrid.Agent;
            var apiKey = allDebrid.ApiKey;

            var urlApi = Url.Combine(BaseUrl, "link/unlock");
            var req = await urlApi
                .SetQueryParam("agent", agent)
                .SetQueryParam("apikey", apiKey)
                .SetQueryParam("link", url)
                .GetJsonAsync<AllDebrid>();

            return req;
            // var urlResult = req.Status == "success" ? req.DebridData.Link.AbsoluteUri : url;
            // Log.Debug("Debrid result: {0}", urlResult);

            // return urlResult;
        }
    }
}