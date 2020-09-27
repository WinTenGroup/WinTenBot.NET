using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Zizi.Bot.Common.JsonSettings
{
    public class CamelCaseFollowProperty:CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.GetCustomAttribute<JsonPropertyAttribute>() is {} jsonProperty)
            {
                property.PropertyName = jsonProperty.PropertyName;
            }

            return property;
        }
    }
}
