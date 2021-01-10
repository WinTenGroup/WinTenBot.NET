using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Zizi.Core.Extensions
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
                return new QueryFactory(connection, compiler);
            });

            return services;
        }
    }
}