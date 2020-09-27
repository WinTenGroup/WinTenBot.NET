using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zizi.Bot.Common.JsonSettings;

namespace Zizi.Bot.Common
{
    public static class Json
    {
        public static string ToJson(this object dataTable, bool indented = false, bool followProperty = false)
        {
            var serializerSetting = new JsonSerializerSettings();

            if (followProperty) serializerSetting.ContractResolver = new CamelCaseFollowProperty();
            serializerSetting.Formatting = indented ? Formatting.Indented : Formatting.None;

            return JsonConvert.SerializeObject(dataTable, serializerSetting);
        }

        public static T MapObject<T>(this string json)
        {
            // return JsonSerializer.Deserialize<T>(json);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static DataTable ToDataTable(this string data)
        {
            return JsonConvert.DeserializeObject<DataTable>(data);
        }

        public static JArray ToArray(this string data)
        {
            return JArray.Parse(data);
        }
    }
}