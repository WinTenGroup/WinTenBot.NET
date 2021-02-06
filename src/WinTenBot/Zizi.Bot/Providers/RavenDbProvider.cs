using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Models;

namespace Zizi.Bot.Providers
{
    [Obsolete("RavenDB will be removed.")]
    public static class RavenDbProvider
    {
        private static DocumentStore Store { get; set; }

        public static void InitDatabase()
        {
            var certPath = BotSettings.RavenDBCertPath;
            var dbName = BotSettings.RavenDBDatabase;
            var nodes = BotSettings.RavenDBNodes;

            Log.Information("Connecting to RavenDB Cloud");
            Log.Debug("Nodes: {0}", nodes.ToJson(true));
            Log.Debug("Cert: {0}", certPath);
            Log.Debug("DBName: {0}", dbName);

            if (!File.Exists(certPath))
            {
                Log.Error("File {0} is missing. RavenDB Disabled", certPath);
                return;
            }

            var clientCertificate = new X509Certificate2(certPath);
            var documentStore = new DocumentStore()
            {
                Urls = nodes.ToArray(),
                Database = dbName,
                Certificate = clientCertificate
            };

            Log.Debug("Initializing client..");
            documentStore.Initialize();
            Store = documentStore;
        }

        public static IRavenQueryable<T> Query<T>()
        {
            var session = Store.OpenSession();
            return session.Query<T>();
        }

        public static IDocumentQuery<T> DocumentQuery<T>()
        {
            using var session = Store.OpenSession();
            var data = session.Advanced.DocumentQuery<T>();
            return data;
        }

        public static void Insert(object obj, string id = null)
        {
            var session = Store.OpenSession();
            if (id != null)
                session.Store(obj, id);
            else
                session.Store(obj);

            session.SaveChanges();
        }

        public static void Delete<T>(T entity)
        {
            var session = Store.OpenSession();
            session.Delete(entity);
            session.SaveChanges();
        }

        public static void Delete(string id)
        {
            var session = Store.OpenSession();
            session.Delete(id);
            session.SaveChanges();
        }
    }
}