using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;

namespace WinTenDev.Zizi.Utils
{
    /// <summary>
    /// SQLKata factory util
    /// </summary>
    public static class SqlKataUtil
    {
        /// <summary>
        /// From table from QueryFactory
        /// </summary>
        /// <param name="queryFactory"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static Query FromTable(this QueryFactory queryFactory, string tableName)
        {
            Log.Debug("Opening table {TableName}", tableName);
            return queryFactory.FromQuery(new Query(tableName));
        }

        /// <summary>
        /// Execute truncate table by Name
        /// </summary>
        /// <param name="queryFactory"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static async Task<int> TruncateTable(this QueryFactory queryFactory, string tableName)
        {
            Log.Debug("Truncate table `{TableName}`", tableName);

            var truncateSql = $"TRUNCATE TABLE {tableName}";
            var rowCount = await queryFactory.RunSqlAsync(truncateSql);

            Log.Debug("Truncate finish");
            return rowCount;
        }

        /// <summary>
        /// Execute raw SQL query
        /// </summary>
        /// <param name="queryFactory"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static async Task<int> RunSqlAsync(this QueryFactory queryFactory, string sql)
        {
            Log.Debug("Executing SQL: \n{Sql}", sql);

            var rowCount = await queryFactory.StatementAsync(sql);
            return rowCount;
        }
    }
}