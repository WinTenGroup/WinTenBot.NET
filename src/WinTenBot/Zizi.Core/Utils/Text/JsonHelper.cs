using Newtonsoft.Json;

namespace Zizi.Core.Utils.Text
{
    public static class JsonHelper
    {
        public static string ToJson<T>(this T tObj, bool indent=false)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = indent ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(tObj);
        }

        public static T MapJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}