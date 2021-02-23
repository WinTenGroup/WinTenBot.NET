using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Embedded;
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

        public static IApplicationBuilder UseEmbeddedRavenDBServer(this IApplicationBuilder app)
        {
            var appConfig = app?.ApplicationServices.GetService<AppConfig>();
            var ravenDbConfig = appConfig?.RavenDBConfig;
            var serverUrl = ravenDbConfig?.Embedded.ServerUrl;
            var frameworkVersion = ravenDbConfig?.Embedded.FrameworkVersion;

            Log.Debug("Starting RavenDB Embedded server.");

            EmbeddedServer.Instance.StartServer(new ServerOptions()
            {
                AcceptEula = true,
                ServerUrl = serverUrl,
                FrameworkVersion = frameworkVersion,
                DataDirectory = "Storage/Data/",
                LogsPath = "Storage/Logs/",
            });

            EmbeddedServer.Instance.ServerProcessExited += delegate(object sender, ServerProcessExitedEventArgs args)
            {
                Log.Error("Server Exited. {0} => {1}", sender, args);
            };

            var pid = EmbeddedServer.Instance.GetServerProcessIdAsync().Result;
            var serverUri = EmbeddedServer.Instance.GetServerUriAsync().Result;

            Log.Debug("RavenDB Embedded server initialized.");
            Log.Debug("ProcessID: {0}", pid);
            Log.Debug("ServerURL: {0}", serverUri);

            return app;
        }
    }
}