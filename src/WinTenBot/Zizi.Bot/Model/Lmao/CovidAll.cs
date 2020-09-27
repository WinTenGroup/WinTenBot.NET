using Newtonsoft.Json;

namespace Zizi.Bot.Model.Lmao
{
    public partial class CovidAll
    {
        public long Cases { get; set; }
        public long TodayCases { get; set; }
        public long Deaths { get; set; }
        public long TodayDeaths { get; set; }
        public long Recovered { get; set; }
        public long TodayRecovered { get; set; }
        public long Updated { get; set; }
        public long Active { get; set; }
        public long Critical { get; set; }
        public decimal CasesPerOneMillion { get; set; }
        public decimal DeathsPerOneMillion { get; set; }
        public long Tests { get; set; }
        public decimal TestsPerOneMillion { get; set; }
        public long Population { get; set; }
    }

    public partial class CovidAll
    {
        public static CovidAll FromJson(string json) => 
            JsonConvert.DeserializeObject<CovidAll>(json, Converter.Settings);
    }

    public static partial class Serialize
    {
        public static string ToJson(this CovidAll self) => 
            JsonConvert.SerializeObject(self, Converter.Settings);
    }
}