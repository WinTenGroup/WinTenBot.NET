using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CloudFlareUtilities;
using Flurl;
using Flurl.Http;
using Serilog;

namespace WinTenDev.Zizi.Utils
{
    public class FastDebridUtil
    {
        public static async Task<string> Convert(string url)
        {
            try
            {
                Log.Debug("Preparing convertion URL: {0}", url);

                var handler = new ClearanceHandler()
                {
                    MaxRetries = 3
                };

                var urlReq = $"https://fastdebrid.tk/get_link.php?link={url}";
                Log.Debug("ReqURL: {0}", urlReq);

                var client = new HttpClient(handler);

                var response = await client.GetAsync(urlReq);

                var content = response.Headers.GetValues("location").First();
                Log.Debug("Location: {0}", content);

                return content;
            }
            catch (AggregateException ex) when (ex.InnerException is CloudFlareClearanceException)
            {
                Log.Error(ex, "Error CloudFlareClearanceException");
                return ex.Message;
                // After all retries, clearance still failed.
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                Log.Error(ex, "Error TaskCanceledException");
                return ex.Message;
                // Looks like we ran into a timeout. Too many clearance attempts?
                // Maybe you should increase client.Timeout as each attempt will take about five seconds.
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Exception");
                return ex.Message;
            }
        }

        public static async Task<string> Convert2(string url)
        {
            var urlReq = await "https://fastdebrid.tk/get_link.php"
                .SetQueryParam("link", url)
                .WithHeader("host", "fastdebrid.tk")
                .GetAsync();
            var headerLocation = urlReq.ResponseMessage.Headers.GetValues("location").First();

            Log.Debug("HeaderLocation: {0}", headerLocation);

            return headerLocation;
        }
    }
}