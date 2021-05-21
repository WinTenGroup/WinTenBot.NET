using Serilog;
using SqlKata;
using SqlKata.Execution;

namespace WinTenDev.Zizi.Utils
{
    public static class SqlKataUtil
    {
        public static Query FromTable(this QueryFactory queryFactory, string tableName)
        {
            Log.Debug("Opening table {TableName}", tableName);
            return queryFactory.FromQuery(new Query(tableName));
        }
    }
}