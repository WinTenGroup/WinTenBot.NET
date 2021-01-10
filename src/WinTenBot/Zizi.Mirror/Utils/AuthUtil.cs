using System.Linq;
using System.Threading.Tasks;
using LiteDB.Async;
using Serilog;

namespace Zizi.Mirror.Utils
{
    public static class AuthUtil
    {
        public static LiteCollectionAsync<AuthorizedChat> GetAuthCollection(this LiteDatabaseAsync db)
        {
            return db.GetCollection<AuthorizedChat>();
        }

        public static async Task<bool> IsAuth(this LiteDatabaseAsync db, long chatId)
        {
            var authChat = await db.GetAuthCollection()
                .Query()
                .ToListAsync();

            var isAuth = authChat.Any(chat => chat.ChatId == chatId);
            Log.Debug("Is ChatID '{0}' authorized? {1}", chatId, isAuth);

            return isAuth;
        }

        public static async Task<bool> SaveAuth(this LiteDatabaseAsync db, AuthorizedChat authorizedChat)
        {
            var authCollection = db.GetAuthCollection();

            return await authCollection.UpsertAsync(authorizedChat);
        }
    }
}