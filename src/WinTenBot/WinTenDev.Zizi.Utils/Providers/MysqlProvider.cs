using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using Serilog;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace WinTenDev.Zizi.Utils.Providers
{
    [Obsolete("This Class will be replaced with QueryFactory from DI")]
    public static class MysqlProvider
    {
        private static string _connStr = "";

        public static void SetConnectionString(string connStr)
        {
            _connStr = connStr;
        }
        public static QueryFactory GetMysqlInstances()
        {
            var connection = new MySqlConnection(_connStr);
            var factory = new QueryFactory(connection, new MySqlCompiler())
            {
                Logger = result =>
                {
                    Log.Debug("MySqlExec: {Result}", result);
                }
            };

            return factory;
        }

        [Obsolete("This method will be replaced with QueryFactory from DI")]
        public static Query ExecForMysql(this Query query, bool printSql = true)
        {
            var connection = new MySqlConnection(_connStr);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sql =>
                {
                    Log.Debug("MySqlExec: {Sql}", sql);
                };
            }

            return factory.FromQuery(query);
        }

        [Obsolete("This method will be replaced with QueryFactory from DI")]
        public static async Task<int> ExecForMysqlNonQueryAsync(this string sql, object param = null,
            bool printSql = false)
        {
            var connection = new MySqlConnection(_connStr);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sqlResult =>
                {
                    Log.Debug("MySqlExec: {SqlResult}", sqlResult);
                };
            }

            return await factory.StatementAsync(sql, param);
        }

        [Obsolete("This method will be replaced with QueryFactory from DI", true)]
        public static int ExecForMysqlNonQuery(this string sql, object param = null, bool printSql = false)
        {
            var connection = new MySqlConnection(_connStr);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sqlResult =>
                {
                    Log.Debug("MySqlExec: {SqlResult}", sqlResult);
                };
            }

            return factory.Statement(sql, param);
        }

        public static async Task<IEnumerable<dynamic>> ExecForMysqlQueryAsync(this string sql, object param = null,
            bool printSql = false)
        {
            var connection = new MySqlConnection(_connStr);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sqlResult =>
                {
                    Log.Debug("MySqlExec: {SqlResult}", sqlResult);
                };
            }

            return await factory.SelectAsync(sql, param);
        }

        public static IEnumerable<dynamic> ExecForMysqlQuery(this string sql, object param = null,
            bool printSql = false)
        {
            var connection = new MySqlConnection(_connStr);
            var factory = new QueryFactory(connection, new MySqlCompiler());
            if (printSql)
            {
                factory.Logger = sqlResult =>
                {
                    Log.Debug("MySqlExec: {SqlResult}", sqlResult);
                };
            }

            return factory.Select(sql, param);
        }

        [Obsolete("This method will be replaced with QueryFactory from DI")]
        public static async Task<int> MysqlDeleteDuplicateRowAsync(this string tableName, string columnKey,
            bool printSql = false)
        {
            Log.Information("Deleting duplicate rows on {TableName}", tableName);

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

            if (printSql) Log.Debug("SQL: {Sql}", sql);

            var exec = await sql.ExecForMysqlNonQueryAsync(sql);
            Log.Information("Deleted: {Exec} rows.", exec);

            return exec;
        }
    }
}