using Serilog;
using SqlKata;
using SqlKata.Execution;

namespace Zizi.Core.Utils
{
    public static class SqlKataUtil
    {
        public static Query FromTable(this QueryFactory queryFactory, string tableName)
        {
            Log.Debug("Opening table {0}", tableName);
            return queryFactory.FromQuery(new Query(tableName));
        }
    }
}