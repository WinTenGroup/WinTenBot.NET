using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using WinTenDev.Zizi.Models.JsonSettings;
using WinTenDev.Zizi.Utils.IO;

namespace WinTenDev.Zizi.Utils.Text
{
    public static class JsonUtil
    {
        private static readonly string workingDir = "Storage/Caches";

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

        public static JArray ToArray(this string data)
        {
            return JArray.Parse(data);
        }

        public static async Task<string> WriteToFileAsync<T>(this T data, string fileJson, bool indented = true)
        {
            var filePath = $"{workingDir}/{fileJson}".EnsureDirectory();
            var json = data.ToJson(indented);

            await json.ToFile(filePath);
            Log.Debug("Writing file complete. FileName: {FilePath}", filePath);

            return filePath;
        }
    }
}