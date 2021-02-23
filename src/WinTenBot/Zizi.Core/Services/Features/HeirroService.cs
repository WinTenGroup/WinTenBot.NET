using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;

namespace Zizi.Core.Services.Features
{
    public class HeirroService
    {
        public async Task<IFlurlResponse> Debrid(string url)
        {
            var baseUrl = "https://rdb.heirro.net/decrypt.php";
            var resp = await baseUrl
                .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0")
                .WithHeader("Referrer","https://rdb.heirro.net/")
                .PostMultipartAsync(content =>
                {
                    content.AddString("linkdl", url);
                });

            return resp;
        }
    }
}