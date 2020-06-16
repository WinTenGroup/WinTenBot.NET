using System;
using Newtonsoft.Json;

namespace WinTenBot.Model.Lmao
{
    public partial class CovidByCountry
    {
        public string Country { get; set; }
        public CountryInfo CountryInfo { get; set; }
        public long Cases { get; set; }
        public long TodayCases { get; set; }
        public long Deaths { get; set; }
        public long TodayDeaths { get; set; }
        public long Recovered { get; set; }
        public long TodayRecovered { get; set; }
        public long Active { get; set; }
        public long Critical { get; set; }
        public long CasesPerOneMillion { get; set; }
        public double DeathsPerOneMillion { get; set; }
        public long Tests { get; set; }
        public decimal TestsPerOneMillion { get; set; }
        public long Population { get; set; }
        public long Updated { get; set; }
    }

    public partial class CountryInfo
    {
        [JsonProperty("_id")]
        public long Id { get; set; }

        [JsonProperty("iso2")]
        public string Iso2 { get; set; }

        [JsonProperty("iso3")]
        public string Iso3 { get; set; }

        [JsonProperty("lat")]
        public long Lat { get; set; }

        [JsonProperty("long")]
        public long Long { get; set; }

        [JsonProperty("flag")]
        public Uri Flag { get; set; }
    }

    public partial class CovidByCountry
    {
        public static CovidByCountry FromJson(string json) => JsonConvert.DeserializeObject<CovidByCountry>(json, Converter.Settings);
    }

    public static partial class Serialize
    {
        public static string ToJson(this CovidByCountry self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}