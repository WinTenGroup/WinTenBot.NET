using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Providers;

namespace Zizi.Bot.Services
{
    public class WordFilterService
    {
        private const string TableName = "word_filter";
        private Message Message { get; set; }
        public WordFilterService(Message message)
        {
            Message = message;
        }

        public async Task<bool> IsExistAsync(Dictionary<string, object> where)
        {
            // var where = new Dictionary<string, object>() {{"chat_id", Message.Chat.Id}};
            
            var check = await new Query(TableName)
                .Where(where)
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);
            
            var isExist = check.Any();

            Log.Debug($"Group setting IsExist: {isExist}");

            return isExist;
        }

        public async Task<bool> SaveWordAsync(string word, bool isGlobal = false, bool deepFilter = false)
        {
            var data = new Dictionary<string,object>()
            {
                {"word",word},
                {"is_global",isGlobal},
                {"deep_filter", deepFilter},
                {"from_id",Message.From.Id},
                {"chat_id",Message.Chat.Id}
            };

            // var isExist = await  IsExistAsync(word);
            // if (isExist) return false;
            
            var insert = await new Query(TableName)
                .ExecForMysql(true)
                .InsertAsync(data)
                .ConfigureAwait(false);
            
            // insert = await new Query(TableName)
            //     .ExecForSqLite(true)
            //     .InsertAsync(data)
            //     .ConfigureAwait(false);

            return insert > 0;

        }
    }
}