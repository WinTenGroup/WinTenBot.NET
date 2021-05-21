using MySqlConnector;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;
using WinTenDev.Zizi.Models.Configs;

namespace WinTenDev.Zizi.Services
{
    public class QueryService
    {
        private readonly ConnectionStrings _connectionStrings;

        public QueryService(ConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }

        public QueryFactory CreateMySqlConnection()
        {
            var mysqlConn = _connectionStrings.MySql;

            var compiler = new MySqlCompiler();
            var connection = new MySqlConnection(mysqlConn);
            var factory = new QueryFactory(connection, compiler)
            {
                Logger = sql =>
                {
                    Log.Debug("MySql Exec: {Sql}", sql);
                }
            };

            return factory;
        }
    }
}