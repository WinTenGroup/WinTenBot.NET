using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Zizi.Bot.Interfaces;
using Zizi.Bot.Services;
using Zizi.Bot.Services.HangfireJobs;

namespace Zizi.Bot.Extensions
{
    public static class CommonServiceExtension
    {
        public static IServiceCollection AddDataService(this IServiceCollection services)
        {
            return services
                .AddScoped<TagsService>();
        }
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