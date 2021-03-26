using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Telegram.Bot.Types;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;

namespace Zizi.Bot.Services.Datas
{
    public class LearningService
    {
        private readonly Message _message;
        private const string TableName = "words_learning";

        public LearningService(Message message)
        {
            _message = message;
        }

        public static bool IsExist(LearnData learnData)
        {
            var select = new Query(TableName)
                .ExecForMysql(true)
                .Where("message", learnData.Message)
                .Get();

            return select.Any();
        }

        public static IEnumerable<dynamic> GetAll(LearnData learnData)
        {
            var select = new Query(TableName)
                .ExecForMysql(true)
                .Get();

            return select;
        }

        public async Task<int> Save(LearnData learnData)
        {
            var insert = await new Query(TableName)
                .ExecForMysql(true)
                .InsertAsync(new Dictionary<string, object>()
                {
                    {"label", learnData.Label},
                    {"message", learnData.Message},
                    {"from_id", _message.From.Id},
                    {"chat_id", _message.Chat.Id}
                });

            Log.Information("Save Learn: {Insert}", insert);

            return insert;
        }
    }
}