﻿using LiteDB.Async;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;
using Zizi.Bot.IO;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Extensions
{
    public static class DataSourceServiceCollectionExtension
    {
        public static IServiceCollection AddSqlKataMysql(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var appConfig = provider.GetRequiredService<AppConfig>();
                var connStr = appConfig.ConnectionStrings.MySql;

                var compiler = new MySqlCompiler();
                var connection = new MySqlConnection(connStr);
                var factory = new QueryFactory(connection, compiler)
                {
                    Logger = sql => { Log.Debug($"MySqlExec: {sql}"); }
                };

                return factory;
            });

            return services;
        }

        public static IServiceCollection AddLiteDb(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var dbPath = "Storage/Data/Local_LiteDB.db";
                Log.Debug("Loading LiteDB: {0}", dbPath);
                var dbName = dbPath.EnsureDirectory();
                var connectionString = $"Filename={dbName};Connection=shared;";

                return new LiteDatabaseAsync(connectionString);
            });

            return services;
        }
    }
}