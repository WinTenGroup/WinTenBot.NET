﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Zizi.Bot.IO;
using Zizi.Bot.Providers;

namespace Zizi.Bot.Tools
{
    [Obsolete("Soon replace with FluentMigration")]
    public static class SqlMigration
    {
        public static async Task MigrateLocalStorage(this string tableName)
        {
            var filePath = Environment.CurrentDirectory + $"/Storage/SQL/Sqlite/{tableName}.sql";
            Log.Debug("Migrating :{FilePath}", filePath);
            await filePath.ExecuteFileForSqLite();
        }

        public static void MigrateMysql()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Storage/SQL/MySql").SanitizeSlash();
            Log.Information("Prepare migrating from SQL: {0}", path);
            var listFiles = Directory.GetFiles(path).Where(f => f.EndsWith(".sql"));
            foreach (var file in listFiles)
            {
                var filePath = file.SanitizeSlash();
                if (filePath.Contains("obs"))
                {
                    Log.Debug("Skip => {FilePath} because obsolete", filePath);
                }
                else
                {
                    Log.Debug("Migrating => {FilePath}", filePath);
                    var sql = File.ReadAllText(file);
                    sql.ExecForMysqlNonQuery(true);
                }
            }

            Log.Information("SQL Migration finish.");
        }

        // public static void MigrateSqlite()
        // {
        //     var path = Environment.CurrentDirectory + @"/Storage/SQL/Sqlite";
        //     var listFiles = Directory.GetFiles(path);
        //     foreach (var file in listFiles)
        //     {
        //         var filePath = file.SanitizeSlash();
        //         Log.Information($"Migrating => {filePath}");
        //         var sql = File.ReadAllText(file);
        //         sql.ExecForSqLite(true);
        //     }
        // }

        public static void MigrateAll()
        {
            MigrateMysql();
            // MigrateSqlite();
        }

        public static void RunMigration()
        {
            Parallel.Invoke(
                async () => await "word_filter".MigrateLocalStorage(),
                async () => await "rss_history".MigrateLocalStorage(),
                async () => await "warn_username_history".MigrateLocalStorage());
        }
    }
}