using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Serilog;
using Zizi.Bot.Models.Pigoora;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Services
{
    public class CekResiService
    {
        private const string CekResiUrl = "https://api.cekresi.pigoora.com/cekResi";
        private readonly AppConfig _appConfig;

        public CekResiService(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        private static List<string> GetCouriers()
        {
            return new List<string> {"sicepat", "jne", "jnt", "ninja", "pos"};
        }

        public void GetResi(string resi)
        {
        }

        public async Task<CekResi> RunCekResi(string resi)
        {
            Log.Information("Checking Resi {0}", resi);
            var couriers = GetCouriers();
            var pigooraConfig = _appConfig.PigooraConfig;
            var cekResiUrl = pigooraConfig.CekResiUrl;
            var cekResiToken = pigooraConfig.CekResiToken;

            CekResi cekResi = null;

            Log.Information("Searching on {0} couriers", couriers.Count);
            foreach (var courier in couriers)
            {
                Log.Information("Searching on courier {0}", courier);
                cekResi = await cekResiUrl
                    .SetQueryParam("key", cekResiToken)
                    .SetQueryParam("resi", resi)
                    .SetQueryParam("kurir", courier)
                    .SetQueryParams()
                    .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0")
                    .WithHeader("Host", "api.cekresi.pigoora.com")
                    .WithHeader("Origin", "https://cekresi.pigoora.com")
                    .WithHeader("Referer", "https://cekresi.pigoora.com/")
                    .GetJsonAsync<CekResi>();

                if (cekResi.Result != null)
                {
                    Log.Warning("Searching resi break!");
                    break;
                }

                await Task.Delay(100);
            }

            Log.Debug("Resi Check finish.");
            return cekResi;
        }
    }
}