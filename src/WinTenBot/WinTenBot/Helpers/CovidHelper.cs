using System;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Serilog;
using WinTenBot.Model.Lmao;
using CovidAll = WinTenBot.Model.CovidAll;

namespace WinTenBot.Helpers
{
    public static class CovidHelper
    {
        private static string CacheFilename
        {
            get
            {
                var timeStamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
                var fileName = $"covid-all-{timeStamp}.json";
                return fileName;
            }
        }

        public static async Task<string> GetCovidUpdatesAsync()
        {
            // var timeStamp = DateTime.Now.ToString("yyyyMMdd-hhmm"); 
            // var fileName = $"covid-all-{timeStamp}.json";
            // var urlApi = "https://coronavirus-tracker-api.herokuapp.com/all";

            await UpdateCacheAsync();

            Log.Information($"Loading cache from {CacheFilename}");
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

        public static async Task<string> GetCovidAll()
        {
            var url = "https://corona.lmao.ninja/all";
            var covidAll = await url.GetJsonAsync<Model.Lmao.CovidAll>();

            var strBuild = new StringBuilder();

            strBuild.AppendLine("<b>Covid 19 Worldwide Updates</b>");
            strBuild.AppendLine($"<b>Cases:</b> {covidAll.Cases}");
            strBuild.AppendLine($"<b>Deaths:</b> {covidAll.Deaths}");
            strBuild.AppendLine($"<b>Recovered:</b> {covidAll.Recovered}");
            strBuild.AppendLine($"<b>Active:</b> {covidAll.Active}");

            var date = DateTimeOffset.FromUnixTimeMilliseconds(covidAll.Updated);
            strBuild.AppendLine($"\n<b>Updated:</b> {date}");
            strBuild.AppendLine($"<b>Source:</b> https://corona.lmao.ninja");

            return strBuild.ToString().Trim();
        }

        public static async Task<string> GetCovidByCountry(string country)
        {
            try
            {
                var urlApi = $"https://corona.lmao.ninja/countries/{country}";
                var covid = await urlApi.GetJsonAsync<CovidByCountry>();

                var strBuild = new StringBuilder();
                strBuild.AppendLine($"<b>Country:</b> {covid.Country}");

                strBuild.AppendLine($"<b>Cases:</b> {covid.Cases}");
                strBuild.AppendLine($"<b>TodayCases:</b> {covid.TodayCases}");

                strBuild.AppendLine($"<b>Deaths:</b> {covid.Deaths}");
                strBuild.AppendLine($"<b>TodayDeaths:</b> {covid.TodayDeaths}");

                strBuild.AppendLine($"<b>Recovered:</b> {covid.Recovered}");
                strBuild.AppendLine($"<b>Active:</b> {covid.Active}");
                strBuild.AppendLine($"<b>Critical:</b> {covid.Critical}");

                strBuild.AppendLine($"<b>Cases Per 1 Milion:</b> {covid.CasesPerOneMillion}");
                strBuild.AppendLine($"<b>Deaths Per 1 Milion:</b> {covid.DeathsPerOneMillion}");

                var date = DateTimeOffset.FromUnixTimeMilliseconds(covid.Updated);
                strBuild.AppendLine($"\n<b>Updated:</b> {date}");
                strBuild.AppendLine($"<b>Source:</b> https://corona.lmao.ninja");

                return strBuild.ToString().Trim();
            }
            catch(Exception ex)
            {
                Log.Error(ex,"Error Getting Covid Info By Region.");
                return "Please check your Country name";
            }
        }

        public static async Task UpdateCacheAsync()
        {
            var urlApi = "https://coronavirus-tracker-api.herokuapp.com/all";

            CacheHelper.ClearCacheOlderThan("covid-all", 1);

            if (!CacheFilename.IsFileCacheExist())
            {
                Log.Information($"Getting information from {urlApi}");
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