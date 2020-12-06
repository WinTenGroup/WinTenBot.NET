using LiteDB.Async;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;
using Zizi.Bot.IO;

namespace Zizi.Bot.Extensions
{
    public static class DataSourceServiceCollectionExtension
    {
        public static IServiceCollection AddSqlKataMysql(
            this IServiceCollection services, string connectionString)
        {
            services.AddScoped(provider =>
            {
                var compiler = new MySqlCompiler();
                var connection = new MySqlConnection(connectionString);
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

        // public static IServiceCollection AddArangoDb(this IServiceCollection services)
        // {
        //     services.AddScoped(x =>
        //     {
        //         var transport = HttpApiTransport.UsingBasicAuth(
        //             new Uri("http://35.231.126.181:8529"),
        //             "zizibot_data",
        //             "zizibot",
        //             "winten7768");
        //
        //         return new ArangoDBClient(transport);
        //     });
        //
        //     return services;
        // }
    }
}