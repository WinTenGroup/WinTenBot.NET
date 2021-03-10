using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zizi.Bot.Models
{
    public partial class CovidAll
    {
        [JsonProperty("confirmed")]
        public Confirmed Confirmed { get; set; }

        [JsonProperty("deaths")]
        public Confirmed Deaths { get; set; }

        [JsonProperty("latest")]
        public Latest Latest { get; set; }

        [JsonProperty("recovered")]
        public Confirmed Recovered { get; set; }
    }

    public class Confirmed
    {
        [JsonProperty("last_updated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("latest")]
        public long Latest { get; set; }

        [JsonProperty("locations")]
        public Location[] Locations { get; set; }

        [JsonProperty("source")]
        public Uri Source { get; set; }
    }

    public class Location
    {
        [JsonProperty("coordinates")]
        public Coordinates Coordinates { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("history")]
        public Dictionary<string, long> History { get; set; }

        [JsonProperty("latest")]
        public long Latest { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }
    }

    public class Coordinates
    {
        [JsonProperty("lat")]
        public string Lat { get; set; }

        [JsonProperty("long")]
        public string Long { get; set; }
    }

    public class Latest
    {
        [JsonProperty("confirmed")]
        public long Confirmed { get; set; }

        [JsonProperty("deaths")]
        public long Deaths { get; set; }

        [JsonProperty("recovered")]
        public long Recovered { get; set; }
    }

    public partial class CovidAll
    {
        public static CovidAll FromJson(string json) => JsonConvert.DeserializeObject<CovidAll>(json, Converter.Settings);
    }

    public static partial class Serialize
    {
        public static string ToJson(this CovidAll self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
    
}