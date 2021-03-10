using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Serilog;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Extensions
{
    public static class RavenDBServiceExtension
    {
        public static IServiceCollection AddRavenDb(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var config = provider.GetService<AppConfig>();
                var documentStore = new DocumentStore();

                if (config == null) return documentStore;

                var ravenDbConfig = config.RavenDBConfig;
                var certPath = ravenDbConfig.CertPath;
                var nodes = ravenDbConfig.Nodes;
                var dbName = ravenDbConfig.DbName;

                documentStore.Urls = nodes.ToArray();
                documentStore.Database = dbName;
                documentStore.Certificate = new X509Certificate2(certPath);

                Log.Debug("Initializing client..");
                documentStore.Initialize();

                return documentStore;
            });

            return services;
        }
    }
}