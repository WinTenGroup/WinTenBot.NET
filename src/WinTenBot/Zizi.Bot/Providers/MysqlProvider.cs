using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Serilog;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using Zizi.Bot.Models;

namespace Zizi.Bot.Providers
{
    public static class MysqlProvider
    {
        public static QueryFactory GetMysqlInstances()
        {
            var connection = new MySqlConnection(BotSettings.DbConnectionString);
            var factory = new QueryFactory(connection, new MySqlCompiler())
            {
                Logger = result => { Log.Debug($"MySqlExec: {result}"); }
            };

            return factory;
        }

        public static Query ExecForMysql(this Query query, bool printSql = true)
        {
            var connection = new MySqlConnection(BotSettings.DbConnectionString);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sql => { Log.Debug($"MySqlExec: {sql}"); };
            }

            return factory.FromQuery(query);
        }

        public static async Task<int> ExecForMysqlNonQueryAsync(this string sql, object param = null,
            bool printSql = false)
        {
            var connection = new MySqlConnection(BotSettings.DbConnectionString);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sqlResult => { Log.Debug($"MySqlExec: {sqlResult}"); };
            }

            return await factory.StatementAsync(sql, param).ConfigureAwait(false);
        }

        public static int ExecForMysqlNonQuery(this string sql, object param = null, bool printSql = false)
        {
            var connection = new MySqlConnection(BotSettings.DbConnectionString);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sqlResult => { Log.Debug($"MySqlExec: {sqlResult}"); };
            }

            return factory.Statement(sql, param);
        }

        public static async Task<IEnumerable<dynamic>> ExecForMysqlQueryAsync(this string sql, object param = null,
            bool printSql = false)
        {
            var connection = new MySqlConnection(BotSettings.DbConnectionString);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sqlResult => { Log.Debug($"MySqlExec: {sqlResult}"); };
            }

            return await factory.SelectAsync(sql, param).ConfigureAwait(false);
        }

        public static IEnumerable<dynamic> ExecForMysqlQuery(this string sql, object param = null,
            bool printSql = false)
        {
            var connection = new MySqlConnection(BotSettings.DbConnectionString);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sqlResult => { Log.Debug($"MySqlExec: {sqlResult}"); };
            }

            return factory.Select(sql, param);
        }

        public static async Task<int> MysqlDeleteDuplicateRowAsync(this string tableName, string columnKey,
            bool printSql = false)
        {
            Log.Information($"Deleting duplicate rows on {tableName}");

            // var sql = $@"DELETE t1 FROM {tableName} t1
            //                 INNER JOIN {tableName} t2
            //                 WHERE 
            //                 t1.id < t2.id AND 
            //                 t1.{columnKey} = t2.{columnKey};".StripLeadingWhitespace();

            var tempTable = $"temp_{tableName}";

            var queries = new StringBuilder();
            queries.AppendLine($"DROP TABLE IF EXISTS {tempTable};");
            queries.AppendLine($"CREATE TABLE {tempTable} SELECT DISTINCT * from {tableName} GROUP BY {columnKey};");
            queries.AppendLine($"DROP TABLE {tableName};");
            queries.AppendLine($"ALTER TABLE {tempTable} RENAME TO {tableName};");

            var sql = queries.ToString();

            if (printSql) Log.Debug($"SQL: {sql}");

            var exec = await sql.ExecForMysqlNonQueryAsync(sql).ConfigureAwait(false);
            Log.Information($"Deleted: {exec} rows.");

            return exec;
        }
    }
}