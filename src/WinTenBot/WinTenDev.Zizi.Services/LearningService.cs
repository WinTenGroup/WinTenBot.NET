using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata.Execution;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Services
{
    public class LearningService
    {
        private readonly QueryFactory _queryFactory;
        private const string TableName = "words_learning";

        public LearningService(QueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public bool IsExist(LearnData learnData)
        {
            var select = _queryFactory.FromTable(TableName)
                .Where("message", learnData.Message)
                .Get();

            return select.Any();
        }

        public IEnumerable<dynamic> GetAll(LearnData learnData)
        {
            var select = _queryFactory.FromTable(TableName).Get();

            return select;
        }

        public async Task<int> Save(LearnData learnData)
        {
            var insert = await _queryFactory.FromTable(TableName)
                .InsertAsync(new Dictionary<string, object>()
                {
                    {"label", learnData.Label},
                    {"message", learnData.Message},
                    {"from_id", learnData.FromId},
                    {"chat_id", learnData.ChatId}
                });

            Log.Information("Save Learn: {Insert}", insert);

            return insert;
        }
    }
}