using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using WinTenBot.IO;
using WinTenBot.Providers;

namespace WinTenBot.Tools
{
    [Obsolete("Soon replace with FluentMigration")]
    public static class SqlMigration
    {
        public static async Task MigrateLocalStorage(this string tableName)
        {
            var filePath = Environment.CurrentDirectory + $"/Storage/SQL/Sqlite/{tableName}.sql";
            Log.Debug($"Migrating :{filePath}");
            await filePath.ExecuteFileForSqLite().ConfigureAwait(false);
        }

        public static void MigrateMysql()
        {
            var path = Environment.CurrentDirectory + @"/Storage/SQL/MySql";
            var listFiles = Directory.GetFiles(path).Where(f => f.EndsWith(".sql"));
            foreach (var file in listFiles)
            {
                var filePath = file.SanitizeSlash();
                if (filePath.Contains("obs"))
                {
                    Log.Information($"Skip => {filePath} because obsolete");
                }
                else
                {
                    Log.Information($"Migrating => {filePath}");
                    var sql = File.ReadAllText(file);
                    sql.ExecForMysqlNonQuery(true);
                }
            }
        }

        public static void MigrateSqlite()
        {
            var path = Environment.CurrentDirectory + @"/Storage/SQL/Sqlite";
            var listFiles = Directory.GetFiles(path);
            foreach (var file in listFiles)
            {
                var filePath = file.SanitizeSlash();
                Log.Information($"Migrating => {filePath}");
                var sql = File.ReadAllText(file); 
                sql.ExecForSqLite(true);
            }
        }

        public static void MigrateAll()
        {
            MigrateMysql();
            MigrateSqlite();
        }

        public static void RunMigration()
        {
            Parallel.Invoke(
                async () => await "word_filter".MigrateLocalStorage()
                    .ConfigureAwait(false),
                async () => await "rss_history".MigrateLocalStorage()
                    .ConfigureAwait(false),
                async () => await "warn_username_history".MigrateLocalStorage()
                    .ConfigureAwait(false));
        }
    }
}