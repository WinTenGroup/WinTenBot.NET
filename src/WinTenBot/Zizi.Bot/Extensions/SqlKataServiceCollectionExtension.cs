using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Zizi.Bot.Extensions
{
    public static class SqlKataServiceCollectionExtension
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
