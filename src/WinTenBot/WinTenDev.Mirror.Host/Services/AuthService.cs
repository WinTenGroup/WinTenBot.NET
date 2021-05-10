using System.Linq;
using System.Threading.Tasks;
using LiteDB.Async;
using Serilog;

namespace WinTenDev.Mirror.Host.Services
{
    public class AuthService
    {
        private readonly LiteDatabaseAsync _liteDatabaseAsync;

        public AuthService(LiteDatabaseAsync liteDatabaseAsync)
        {
            _liteDatabaseAsync = liteDatabaseAsync;
        }

        public LiteCollectionAsync<AuthorizedChat> GetAuthCollection()
        {
            return _liteDatabaseAsync.GetCollection<AuthorizedChat>();
        }

        public async Task<bool> IsAuth(long chatId)
        {
            var authChat = await GetAuthCollection()
                .Query()
                .ToListAsync();

            var isAuth = authChat.Any(chat => chat.ChatId == chatId && chat.IsAuthorized);
            Log.Debug("Is ChatID '{0}' authorized? {1}", chatId, isAuth);

            return isAuth;
        }

        public async Task UnAuth(AuthorizedChat authorizedChat = null)
        {
            var authCollection = GetAuthCollection();
            if (authorizedChat == null)
            {
                Log.Debug("Unauthorizing all chat..");
                await authCollection.DeleteAllAsync();
            }
            else
            {
                Log.Debug("Unauthorizing {0}", authorizedChat.ChatId);
                await authCollection.DeleteManyAsync(chat => chat.ChatId == authorizedChat.ChatId);
            }

            Log.Debug("Unauthorizing finish..");
        }

        public async Task SaveAuth(AuthorizedChat authorizedChat)
        {
            var authCollection = GetAuthCollection();
            var lists = await authCollection.Query().ToListAsync();

            if (lists.Any(chat => chat.ChatId == authorizedChat.ChatId))
            {
                await authCollection.DeleteManyAsync(chat => chat.ChatId == authorizedChat.ChatId);
                await authCollection.InsertAsync(authorizedChat);

            }

            await authCollection.InsertAsync(authorizedChat);
        }
    }
}