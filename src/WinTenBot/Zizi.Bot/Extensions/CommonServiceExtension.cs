using Dapper;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Zizi.Bot.Extensions
{
    public static class CommonServiceExtension
    {
        public static IApplicationBuilder ConfigureNewtonsoftJson(this IApplicationBuilder app)
        {
            var contractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                ContractResolver = contractResolver,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return app;
        }

        public static IApplicationBuilder ConfigureDapper(this IApplicationBuilder app)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            return app;
        }
    }
}