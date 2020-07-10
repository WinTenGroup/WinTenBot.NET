using System;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WinTenBot.Migrations.MySql;

namespace WinTenBot.Tools
{
    public static class DbMigration
    {
        public static string ConnectionString { get; set; }

        public static void RunMySqlMigration()
        {
            var serviceProvider = CreateMysqlServices();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (var scope = serviceProvider.CreateScope())
            {
                UpdateMySqlDatabase(scope.ServiceProvider);
            }
        }

        /// <summary>
        /// Configure the dependency injection services
        /// </summary>
        private static IServiceProvider CreateMysqlServices()
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddMySql5()
                    .WithGlobalConnectionString(ConnectionString)
                    .ScanIn(typeof(CreateTableAfk).Assembly).For.Migrations()
                    .ScanIn(typeof(CreateTableChatSettings).Assembly).For.Migrations()
                    .ScanIn(typeof(CreateTableGlobalBan).Assembly).For.Migrations()
                    .ScanIn(typeof(CreateTableRssHistory).Assembly).For.Migrations()
                    .ScanIn(typeof(CreateTableRssSettings).Assembly).For.Migrations()
                    .ScanIn(typeof(CreateTableSpells).Assembly).For.Migrations()
                    .ScanIn(typeof(CreateTableTags).Assembly).For.Migrations()
                    .ScanIn(typeof(CreateTableWordsLearning).Assembly).For.Migrations()
                )
                .AddLogging(lb => lb.AddSerilog())
                .BuildServiceProvider(false);
        }

        /// <summary>
        /// Update the database
        /// </summary>
        private static void UpdateMySqlDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp();
        }
    }
}