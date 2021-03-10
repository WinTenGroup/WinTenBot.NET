using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zizi.Bot.Models.Pigoora
{
    public partial class CekResi
    {
        [JsonProperty("app")]
        public string App { get; set; }

        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    public class Result
    {
        [JsonProperty("delivered")]
        public bool Delivered { get; set; }

        [JsonProperty("summary")]
        public Summary Summary { get; set; }

        [JsonProperty("details")]
        public Details Details { get; set; }

        [JsonProperty("manifest")]
        public List<Manifest> Manifest { get; set; }

        [JsonProperty("delivery_status")]
        public DeliveryStatus DeliveryStatus { get; set; }
    }

    public class DeliveryStatus
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("pod_receiver")]
        public string PodReceiver { get; set; }

        [JsonProperty("pod_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime PodDate { get; set; }

        [JsonProperty("pod_time", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime PodTime { get; set; }
    }

    public class Details
    {
        [JsonProperty("waybill_number")]
        public string WaybillNumber { get; set; }

        [JsonProperty("waybill_date")]
        public DateTimeOffset WaybillDate { get; set; }

        [JsonProperty("waybill_time")]
        public DateTimeOffset WaybillTime { get; set; }

        [JsonProperty("weight")]
        public string Weight { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("shippper_name")]
        public string ShippperName { get; set; }

        [JsonProperty("shipper_address1")]
        public string ShipperAddress1 { get; set; }

        [JsonProperty("shipper_address2")]
        public string ShipperAddress2 { get; set; }

        [JsonProperty("shipper_address3")]
        public string ShipperAddress3 { get; set; }

        [JsonProperty("shipper_city")]
        public string ShipperCity { get; set; }

        [JsonProperty("receiver_name")]
        public string ReceiverName { get; set; }

        [JsonProperty("receiver_address1")]
        public string ReceiverAddress1 { get; set; }

        [JsonProperty("receiver_address2")]
        public string ReceiverAddress2 { get; set; }

        [JsonProperty("receiver_address3")]
        public string ReceiverAddress3 { get; set; }

        [JsonProperty("receiver_city")]
        public string ReceiverCity { get; set; }
    }

    public class Manifest
    {
        [JsonProperty("manifest_code")]
        public string ManifestCode { get; set; }

        [JsonProperty("manifest_description")]
        public string ManifestDescription { get; set; }

        [JsonProperty("manifest_date")]
        public DateTimeOffset ManifestDate { get; set; }

        [JsonProperty("manifest_time")]
        public DateTimeOffset ManifestTime { get; set; }

        [JsonProperty("city_name")]
        public string CityName { get; set; }
    }

    public class Summary
    {
        [JsonProperty("courier_code")]
        public string CourierCode { get; set; }

        [JsonProperty("courier_name")]
        public string CourierName { get; set; }

        [JsonProperty("waybill_number")]
        public string WaybillNumber { get; set; }

        [JsonProperty("service_code")]
        public string ServiceCode { get; set; }

        [JsonProperty("waybill_date")]
        public DateTimeOffset WaybillDate { get; set; }

        [JsonProperty("shipper_name")]
        public string ShipperName { get; set; }

        [JsonProperty("receiver_name")]
        public string ReceiverName { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public partial class CekResi
    {
        public static CekResi FromJson(string json) => JsonConvert.DeserializeObject<CekResi>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this CekResi self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
            },
        };
    }
}