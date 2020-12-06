using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Raven.Embedded;
using Serilog;

namespace Zizi.Bot.Tools.RavenDbUtils
{
    public static class EmbeddedInstanceUtil
    {
        public static async Task<IDocumentSession> StoreData<T>(T entity, string id)
        {
            var nameOf = typeof(T);
            Log.Debug("Storing data. NameOf: {0}", nameOf);

            var store = await EmbeddedServer.Instance.GetDocumentStoreAsync("caches")
                .ConfigureAwait(false);

            var session = store.OpenSession();
            session.Store(entity, id);

            return session;
        }

        public static async Task<IAsyncDocumentSession> StoreAndSave<T>(T entity, string id)
        {
            var nameOf = typeof(T);
            Log.Debug("Storing data. NameOf: {0}", nameOf);

            var store = await EmbeddedServer.Instance.GetDocumentStoreAsync("caches")
                .ConfigureAwait(false);
            var bulk = store.BulkInsert();

            var session = store.OpenAsyncSession();

            await bulk.StoreAsync(entity).ConfigureAwait(false);

            // await session.StoreAsync(entity, id).ConfigureAwait(false);
            // await session.SaveChangesAsync().ConfigureAwait(false);

            return session;
        }
    }
}