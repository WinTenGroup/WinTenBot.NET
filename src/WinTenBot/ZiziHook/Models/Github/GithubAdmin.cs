#region

using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace ZiziHook.Models.Github
{
    public partial class GithubAdmin
    {
        [JsonProperty("zen")]
        public string Zen
        {
            get; set;
        }

        [JsonProperty("hook_id")]
        public long HookId
        {
            get; set;
        }

        [JsonProperty("hook")]
        public Hook Hook
        {
            get; set;
        }

        [JsonProperty("repository")]
        public Repository Repository
        {
            get; set;
        }

        [JsonProperty("sender")]
        public Sender Sender
        {
            get; set;
        }
    }

    public partial class GithubAdmin
    {
        public static GithubAdmin FromJson(string json)
        {
            return JsonConvert.DeserializeObject<GithubAdmin>(json, Converter.Settings);
        }
    }

    public static class Serialize
    {
        public static string ToJson(this GithubAdmin self)
        {
            return JsonConvert.SerializeObject(self, Converter.Settings);
        }
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
            }
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public static readonly ParseStringConverter Singleton = new ParseStringConverter();

        public override bool CanConvert(Type t)
        {
            return t == typeof(long) || t == typeof(long?);
        }

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (long.TryParse(value, out l))
                return l;
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
        }
    }
}