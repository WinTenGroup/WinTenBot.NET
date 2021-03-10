using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Zizi.Bot.Providers
{
    [Obsolete("SQLite no longer used anymore")]
    public static class SqliteProvider
    {
        static readonly string dbPath = "Storage/Common/LocalStorage.db";

        [Obsolete("SQLite no longer used anymore")]
        private static SQLiteConnection InitSqLite()
        {
            var connBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath, JournalMode = SQLiteJournalModeEnum.Memory, Version = 3
            };
            var connStr = connBuilder.ConnectionString;
            
            if (File.Exists(dbPath)) return new SQLiteConnection(connStr);

            Log.Information("Creating {DbPath} for LocalStorage", dbPath);
            SQLiteConnection.CreateFile(dbPath);

            return new SQLiteConnection(connStr);
        }

        [Obsolete("SQLite no longer used anymore")]
        public static Query ExecForSqLite(this Query query, bool printSql = false)
        {
            var connection = InitSqLite();
            
            var factory = new QueryFactory(connection, new SqliteCompiler());

            if (printSql) factory.Logger = sqlResult =>
            {
                Log.Debug("SQLiteExec: {SqlResult}", sqlResult);
            };

            return factory.FromQuery(query);
        }

        [Obsolete("SQLite no longer used anymore")]
        public static async Task<int> ExecForSqLite(this string sql, bool printSql = false, object param = null)
        {
           var connection = InitSqLite();
            
            var factory = new QueryFactory(connection, new SqliteCompiler());

            if (printSql) factory.Logger = sqlResult =>
            {
                Log.Debug("SQLiteExec: {SqlResult}", sqlResult);
            };

            return await factory.StatementAsync(sql, param)
                .ConfigureAwait(false);
        }

        public static async Task<IEnumerable<dynamic>> ExecForSqLiteQuery(this string sql, bool printSql = false, object param = null)
        {
            var connection = InitSqLite();

            var factory = new QueryFactory(connection, new SqliteCompiler());

            if (printSql) factory.Logger = sqlResult =>
            {
                Log.Debug("SQLiteExec: {SqlResult}", sqlResult);
            };

            return await factory.SelectAsync(sql, param)
                .ConfigureAwait(false);
        }

        public static async Task<int> DeleteDuplicateRow(this string tableName, string columnKey)
        {
            Log.Information("Deleting duplicate row(s)");
            var sql = $"DELETE FROM {tableName} " +
                      "WHERE rowid NOT IN( " +
                      "SELECT min(rowid) " +
                      $"FROM {tableName} " +
                      $"GROUP BY {columnKey});";

            var result = await sql.ExecForSqLite(true)
                .ConfigureAwait(false);
            Log.Information("Deleted {Result}", result);

            return result;
        }

        public static bool IfTableExist(this string tableName)
        {
            var query = new Query("sqlite_master")
                .Where("type","table")
                .Where("name",tableName)
                .ExecForSqLite(true)
                .Get();
            
            var isExist = query.Any();

            Log.Debug("Is {TableName} exist: {IsExist}", tableName, isExist);
            
            return isExist;
        }

        public static async Task<bool> ExecuteFileForSqLite(this string filePath)
        {
            if (!File.Exists(filePath)) return false;
            
            var sql = await File.ReadAllTextAsync(filePath)
                .ConfigureAwait(false);
            await sql.ExecForSqLite(true)
                .ConfigureAwait(false);

            return true;
        }
    }
}