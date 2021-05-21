using System;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Serilog;
using WinTenDev.Zizi.Models.Lmao;
using WinTenDev.Zizi.Utils;
using CovidAll = WinTenDev.Zizi.Models.Types.CovidAll;

namespace WinTenDev.Zizi.Services
{
    public class LmaoCovidService
    {
        private string CacheFilename
        {
            get
            {
                var timeStamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
                var fileName = $"covid-all-{timeStamp}.json";
                return fileName;
            }
        }

        public async Task<string> GetCovidUpdatesAsync()
        {
            // var timeStamp = DateTime.Now.ToString("yyyyMMdd-hhmm"); 
            // var fileName = $"covid-all-{timeStamp}.json";
            // var urlApi = "https://coronavirus-tracker-api.herokuapp.com/all";

            await UpdateCacheAsync();

            Log.Information("Loading cache from {CacheFilename}", CacheFilename);
            var covidAll = await CacheFilename.ReadCacheAsync<CovidAll>();

            // Log.Information($"CovidAll: {covidAll.ToJson(true)}");

            var confirmed = covidAll.Confirmed;
            var deaths = covidAll.Deaths;
            var recovered = covidAll.Recovered;

            var messageBuild = new StringBuilder();

            messageBuild.AppendLine("Corona Updates.");
            messageBuild.AppendLine("1. Confirmed");
            messageBuild.AppendLine($"LastUpdate: {confirmed.LastUpdated}");
            messageBuild.AppendLine($"Latest: {confirmed.Latest}");
            messageBuild.AppendLine();

            messageBuild.AppendLine("2. Deaths");
            messageBuild.AppendLine($"LastUpdate: {deaths.LastUpdated}");
            messageBuild.AppendLine($"Latest: {deaths.Latest}");
            messageBuild.AppendLine();

            messageBuild.AppendLine("3. Recovered");
            messageBuild.AppendLine($"LastUpdate: {recovered.LastUpdated}");
            messageBuild.AppendLine($"Latest: {recovered.Latest}");

            messageBuild.AppendLine();
            messageBuild.Append($"Source: {confirmed.Source}");

            return messageBuild.ToString().Trim();
        }

        public async Task<string> GetCovidAll()
        {
            Log.Information("Send request API.");
            var url = "https://corona.lmao.ninja/v2/all";
            var covid = await url.GetJsonAsync<Zizi.Models.Lmao.CovidAll>();

            Log.Information("Building result");
            var strBuild = new StringBuilder();
            strBuild.AppendLine("<b>Covid 19 Worldwide Updates</b>");
            strBuild.AppendLine($"<b>Cases:</b> {covid.Cases:#,#}");
            strBuild.AppendLine($"<b>Today Cases:</b> {covid.TodayCases:#,#}");

            strBuild.AppendLine($"<b>Deaths:</b> {covid.Deaths:#,#}");
            strBuild.AppendLine($"<b>Today Deaths:</b> {covid.TodayDeaths:#,#}");

            strBuild.AppendLine($"<b>Recovered:</b> {covid.Recovered:#,#}");
            strBuild.AppendLine($"<b>Today Recovered:</b> {covid.TodayRecovered:#,#}");

            strBuild.AppendLine($"<b>Active:</b> {covid.Active:#,#}");
            strBuild.AppendLine($"<b>Critical:</b> {covid.Critical:#,#}");

            strBuild.AppendLine($"<b>Cases per 1 M:</b> {covid.CasesPerOneMillion.NumSeparator()}");
            strBuild.AppendLine($"<b>Deaths per 1 M:</b> {covid.DeathsPerOneMillion.NumSeparator()}");

            strBuild.AppendLine($"<b>Tests:</b> {covid.Tests:#,#}");
            strBuild.AppendLine($"<b>Tests per 1 M:</b> {covid.TestsPerOneMillion.NumSeparator()}");
            strBuild.AppendLine($"<b>Total Polulation:</b> {covid.Population:#,#}");

            var date = DateTimeOffset.FromUnixTimeMilliseconds(covid.Updated);
            strBuild.AppendLine($"\n<b>Updated:</b> {date}");
            strBuild.AppendLine($"<b>Source:</b> https://corona.lmao.ninja");

            strBuild.AppendLine();
            strBuild.AppendLine("<b>Covid info by Country.</b>");
            strBuild.AppendLine("<code>/covid [country name]</code>");

            return strBuild.ToString().Trim();
        }

        public async Task<string> GetCovidByCountry(string country)
        {
            try
            {
                var urlApi = $"https://corona.lmao.ninja/v2/countries/{country}";
                var covid = await urlApi.GetJsonAsync<CovidByCountry>();

                var strBuild = new StringBuilder();
                strBuild.AppendLine($"<b>Country:</b> {covid.Country}");

                strBuild.AppendLine($"<b>Cases:</b> {covid.Cases:#,#}");
                strBuild.AppendLine($"<b>Today Cases:</b> {covid.TodayCases:#,#}");

                strBuild.AppendLine($"<b>Deaths:</b> {covid.Deaths:#,#}");
                strBuild.AppendLine($"<b>Today Deaths:</b> {covid.TodayDeaths:#,#}");

                strBuild.AppendLine($"<b>Recovered:</b> {covid.Recovered:#,#}");
                strBuild.AppendLine($"<b>Today Recovered:</b> {covid.TodayRecovered:#,#}");

                strBuild.AppendLine($"<b>Active:</b> {covid.Active:#,#}");
                strBuild.AppendLine($"<b>Critical:</b> {covid.Critical:#,#}");

                strBuild.AppendLine($"<b>Cases per 1 M:</b> {covid.CasesPerOneMillion.NumSeparator()}");
                strBuild.AppendLine($"<b>Deaths per 1 M:</b> {covid.DeathsPerOneMillion.NumSeparator()}");

                strBuild.AppendLine($"<b>Tests:</b> {covid.Tests:#,#}");
                strBuild.AppendLine($"<b>Tests per 1 M:</b> {covid.TestsPerOneMillion.NumSeparator()}");
                strBuild.AppendLine($"<b>Total Polulation:</b> {covid.Population:#,#}");

                var date = DateTimeOffset.FromUnixTimeMilliseconds(covid.Updated);
                strBuild.AppendLine($"\n<b>Updated:</b> {date}");
                strBuild.AppendLine($"<b>Source:</b> https://corona.lmao.ninja");

                return strBuild.ToString().Trim();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error Getting Covid Info By Region.");
                return "Please check your Country name";
            }
        }

        public async Task UpdateCacheAsync()
        {
            var urlApi = "https://coronavirus-tracker-api.herokuapp.com/all";

            CachingUtil.ClearCacheOlderThan("covid-all", 1);

            if (!CacheFilename.IsFileCacheExist())
            {
                Log.Information("Getting information from {UrlApi}", urlApi);
                var covidAll = await urlApi.GetJsonAsync<CovidAll>();

                await covidAll.WriteCacheAsync(CacheFilename);
            }
            else
            {
                Log.Information("Covid Cache has updated.");
            }
        }
    }
}