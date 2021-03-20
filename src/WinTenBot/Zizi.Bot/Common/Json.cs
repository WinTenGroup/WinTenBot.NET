using System;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Zizi.Bot.Common.JsonSettings;
using Zizi.Bot.IO;

namespace Zizi.Bot.Common
{
    public static class Json
    {
        private static readonly string workingDir = "Storage/Caches";

        [Obsolete("Please use with type T.")]
        public static string ToJson(this object dataTable, bool indented = false, bool followProperty = false)
        {
            var serializerSetting = new JsonSerializerSettings();

            if (followProperty) serializerSetting.ContractResolver = new CamelCaseFollowProperty();
            serializerSetting.Formatting = indented ? Formatting.Indented : Formatting.None;

            return JsonConvert.SerializeObject(dataTable, serializerSetting);
        }

        public static string ToJson<T>(this T data, bool indented = false, bool followProperty = false)
        {
            var serializerSetting = new JsonSerializerSettings();

            if (followProperty) serializerSetting.ContractResolver = new CamelCaseFollowProperty();
            serializerSetting.Formatting = indented ? Formatting.Indented : Formatting.None;

            return JsonConvert.SerializeObject(data, serializerSetting);
        }

        public static T MapObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        [Obsolete("Please use MapObject<DataTable>")]
        public static DataTable ToDataTable(this string data)
        {
            return JsonConvert.DeserializeObject<DataTable>(data);
        }

        public static JArray ToArray(this string data)
        {
            return JArray.Parse(data);
        }

        public static async Task<string> WriteToFileAsync<T>(this T data, string fileJson, bool indented = true)
        {
            var filePath = $"{workingDir}/{fileJson}".EnsureDirectory();
            var json = data.ToJson(indented);

            await json.ToFile(filePath);
            Log.Debug("Writing file complete. FileName: {0}", filePath);

            return filePath;
        }
    }
}