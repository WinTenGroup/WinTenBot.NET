using ClickHouse.Client.ADO;
using LiteDB.Async;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using MySqlConnector.Logging;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;
using Zizi.Bot.IO;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Extensions
{
    public static class DataSourceServiceExtension
    {
        public static IServiceCollection AddSqlKataMysql(this IServiceCollection services)
        {
            MySqlConnectorLogManager.Provider = new SerilogLoggerProvider();

            services.AddScoped(provider =>
            {
                var appConfig = provider.GetRequiredService<AppConfig>();
                var connStr = appConfig.ConnectionStrings.MySql;

                var compiler = new MySqlCompiler();
                var connection = new MySqlConnection(connStr);
                var factory = new QueryFactory(connection, compiler)
                {
                    Logger = sql =>
                    {
                        Log.Debug("MySql Exec: {0}", sql);
                    }
                };

                return factory;
            });

            return services;
        }

        public static IServiceCollection AddLiteDb(this IServiceCollection services)
        {
            services.AddScoped(_ =>
            {
                var dbPath = "Storage/Data/Local_LiteDB.db";
                Log.Debug("Loading LiteDB: {0}", dbPath);
                var dbName = dbPath.EnsureDirectory();
                var connectionString = $"Filename={dbName};Connection=shared;";

                return new LiteDatabaseAsync(connectionString);
            });

            return services;
        }

        public static IServiceCollection AddClickHouse(this IServiceCollection services)
        {
            return services.AddScoped(provide =>
            {
                var appConfig = provide.GetRequiredService<AppConfig>();

                var connStr = appConfig.ConnectionStrings.ClickHouseConn;

                return new ClickHouseConnection(connStr);
            });
        }
    }
}