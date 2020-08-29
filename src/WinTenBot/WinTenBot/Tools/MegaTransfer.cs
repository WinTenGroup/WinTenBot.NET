using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;
using Serilog;
using WinTenBot.Common;
using WinTenBot.Services;

namespace WinTenBot.Tools
{
    public static class MegaTransfer
    {
        public static bool IsMegaUrl(this string url)
        {
            if (!url.IsValidUrl()) return false;
            
            var uri = new Uri(url);
            return uri.Host.Contains("mega.nz");
        }

        public static async Task DownloadFileAsync(this TelegramService telegramService, string megaUrl)
        {
            try
            {
                double downloadProgress = 0;
                var swUpdater = Stopwatch.StartNew();
                Log.Information("Starting download from Mega. Url: {0}", megaUrl);
                var client = new MegaApiClient();
                await client.LoginAnonymousAsync().ConfigureAwait(false);

                var fileLink = new Uri(megaUrl);
                var node = await client.GetNodeFromLinkAsync(fileLink).ConfigureAwait(false);

                Log.Debug("Downloading {0}", node.Name);
                IProgress<double> progressHandler = new Progress<double>(async x =>
                {
                    downloadProgress = x;
                    
                    if (swUpdater.Elapsed.Seconds < 5) return;
                    swUpdater.Restart();
                    await telegramService.EditAsync($"Downloading Progress: {downloadProgress:.##} %")
                        .ConfigureAwait(false);
                    // swUpdater.Start();
                });
                
                await client.DownloadFileAsync(fileLink, node.Name, progressHandler)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await telegramService.SendTextAsync($"🚫 <b>Sesuatu telah terjadi.</b>\n{ex.Message}")
                    .ConfigureAwait(false);
            }
        }
    }
}