using System;
using Newtonsoft.Json;

namespace Zizi.Bot.Model
{
   public partial class HitActivity
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("via_bot")]
        public string ViaBot { get; set; }

        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("from_id")]
        public long FromId { get; set; }

        [JsonProperty("from_first_name")]
        public string FromFirstName { get; set; }

        [JsonProperty("from_last_name")]
        public string FromLastName { get; set; }

        [JsonProperty("from_username")]
        public string FromUsername { get; set; }

        [JsonProperty("from_lang_code")]
        public string FromLangCode { get; set; }

        [JsonProperty("chat_id")]
        public string ChatId { get; set; }

        [JsonProperty("chat_username")]
        public object ChatUsername { get; set; }

        [JsonProperty("chat_type")]
        public string ChatType { get; set; }

        [JsonProperty("chat_title")]
        public string ChatTitle { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    // public partial class HitActivity
    // {
    //     public static HitActivity[] FromJson(string json) 
    //         => JsonConvert.DeserializeObject<HitActivity[]>(json, Converter.Settings);
    // }
    //
    // public static partial class Serialize
    // {
    //     public static string ToJson(this HitActivity[] self) 
    //         => JsonConvert.SerializeObject(self, Converter.Settings);
    // }

    // internal static class Converter
    // {
    //     public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    //     {
    //         MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
    //         DateParseHandling = DateParseHandling.None,
    //         Converters =
    //         {
    //             new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
    //         },
    //     };
    // }

    // internal class ParseStringConverter : JsonConverter
    // {
    //     public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);
    //
    //     public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //     {
    //         if (reader.TokenType == JsonToken.Null) return null;
    //         var value = serializer.Deserialize<string>(reader);
    //         long l;
    //         if (Int64.TryParse(value, out l))
    //         {
    //             return l;
    //         }
    //         throw new Exception("Cannot unmarshal type long");
    //     }
    //
    //     public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //     {
    //         if (untypedValue == null)
    //         {
    //             serializer.Serialize(writer, null);
    //             return;
    //         }
    //         var value = (long)untypedValue;
    //         serializer.Serialize(writer, value.ToString());
    //         return;
    //     }
    //
    //     public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    // }
}