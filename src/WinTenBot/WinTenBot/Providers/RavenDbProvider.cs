using System.Security.Cryptography.X509Certificates;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using WinTenBot.IO;

namespace WinTenBot.Providers
{
    public static class RavenDbProvider
    {
        private static DocumentStore Store { get; set; }

        public static void InitDatabase()
        {
            var clientCertificate = new X509Certificate2(@"C:\Users\Azhe Kun\Documents\Projeks\free.azhe403.client.certificate.pfx");
            var documentStore = new DocumentStore()
            {
                Urls = new[] {"https://a.free.azhe403.ravendb.cloud"},
                Database = "zizi_cache_dev",
                Certificate = clientCertificate
            };

            documentStore.Initialize();
            Store = documentStore;
        }

        public static IRavenQueryable<T> Query<T>()
        {
            // var collectionName = typeof(T).Name.Pluralize();
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