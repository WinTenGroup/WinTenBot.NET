using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Models.Uptobox;
using Zizi.Bot.Services;
using Zizi.Bot.Telegram;
using Url = Flurl.Url;

namespace Zizi.Bot.Tools
{
    public static class UptoboxUtil
    {
        private const string UptoboxApi = "https://uptobox.com/api";

        public static bool IsUptoboxUrl(this string url)
        {
            if (!url.IsValidUrl()) return false;

            var uri = new Uri(url);
            return "uptobox.com".Contains(uri.Host, StringComparison.InvariantCultureIgnoreCase);
        }

        public static async Task<UptoboxUser> GetMe()
        {
            Log.Information("Getting Uptobox Me..");
            var token = BotSettings.UptoboxToken;
            if (token.IsNullOrEmpty())
            {
                Log.Information("Uptobox disabled because Token is not configured.");
                return null;
            }

            var url = Url.Combine(UptoboxApi, "user/me");
            var json = await url
                .SetQueryParam("token", token)
                .GetJsonAsync<UptoboxUser>()
                .ConfigureAwait(false);
            // Log.Debug("Uptobox Me: {0}", json.ToJson(true));

            return json;
        }

        public static async Task<string> GetDownloadLinkAsync(string fileId)
        {
            Log.Information("Getting download Link. FileID: {0}", fileId);
            var url = Url.Combine(UptoboxApi, "link");
            var token = BotSettings.UptoboxToken;

            var req = url
                .SetQueryParam("token", token)
                .SetQueryParam("file_code", fileId);

            var waiting = await req.GetJsonAsync<UptoboxLink>().ConfigureAwait(false);
            Log.Debug("Waiting: {0}", waiting.ToJson(true));

            return waiting.LinkData.DlLink;
        }

        public static async Task DownloadUrlAsync(this TelegramService telegramService)
        {
            try
            {
                var message = telegramService.Message;
                var messageText = message.Text.GetTextWithoutCmd();
                var chatId = message.Chat.Id;
                var partsText = messageText.Split(" ");
                var param1 = partsText.ValueOfIndex(0);
                Log.Information("Starting download from Uptobox. Url: {0}", param1);

                var user = await GetMe().ConfigureAwait(false);
                var isPremium = user.UserData.Premium;
                if (!isPremium)
                {
                    Log.Information("Uptobox can't be continue because Token isn't premium.");
                    return;
                }

                var fileId = param1.Replace("https://uptobox.com/", "", StringComparison.InvariantCulture).Trim();
                var downloadLink = await GetDownloadLinkAsync(fileId).ConfigureAwait(false);

                await telegramService.DownloadFile(downloadLink).ConfigureAwait(false);
            }
            catch
            {
                await telegramService.EditAsync("Terjadi kesalahan ketika mengunduh file dari Uptobox. Pastikan kembali tautan.").ConfigureAwait(false);
            }
        }
    }
}