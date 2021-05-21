﻿using System;
using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Lmao
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

    public class CountryInfo
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
}