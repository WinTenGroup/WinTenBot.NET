using System;
using System.Reflection;
using FluentMigrator.Builders;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using WinTenDev.Zizi.Models.Configs;

namespace WinTenDev.Zizi.Host.Extensions
{
    public static class FluentMigrationExtension
    {
        public static IServiceCollection AddFluentMigration(this IServiceCollection services)
        {
            var connectionStrings = services.BuildServiceProvider()
                .GetRequiredService<IOptionsSnapshot<ConnectionStrings>>().Value;

            var connStr = connectionStrings.MySql;

            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                        .AddMySql5()
                        .WithGlobalConnectionString(connStr)
                        .ScanIn(Assembly.GetExecutingAssembly()).For.All()

                // .ScanIn(typeof(CreateTableAfk).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableChatSettings).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableGlobalBan).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableHitActivity).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableRssHistory).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableRssSettings).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableSafeMember).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableSpells).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableTags).Assembly).For.Migrations()
                // .ScanIn(typeof(CreateTableWordsLearning).Assembly).For.Migrations()
                )
                .AddLogging(lb => lb.AddSerilog())
                .Configure<LogFileFluentMigratorLoggerOptions>(options =>
                {
                    options.ShowSql = true;
                });

            return services;
        }

        public static IApplicationBuilder UseFluentMigration(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;
            var scopes = services.CreateScope();
            var runner = scopes.ServiceProvider.GetRequiredService<IMigrationRunner>();

            Log.Information("Running DB migration..");

            runner.ListMigrations();

            Log.Debug("Running MigrateUp");
            runner.MigrateUp();

            return app;
        }

        public static TNext AsMySqlText<TNext>(this IColumnTypeSyntax<TNext> createTableColumnAsTypeSyntax)
            where TNext : IFluentSyntax
        {
            return createTableColumnAsTypeSyntax.AsCustom("TEXT");
        }

        public static TNext AsMySqlMediumText<TNext>(this IColumnTypeSyntax<TNext> createTableColumnAsTypeSyntax)
            where TNext : IFluentSyntax
        {
            return createTableColumnAsTypeSyntax.AsCustom("MEDIUMTEXT");
        }

        public static TNext AsMySqlVarchar<TNext>(this IColumnTypeSyntax<TNext> columnTypeSyntax, Int16 max)
            where TNext : IFluentSyntax
        {
            string varcharType = $"VARCHAR({max}) COLLATE utf8mb4_bin";
            return columnTypeSyntax.AsCustom(varcharType);
        }

        public static TNext AsMySqlTimestamp<TNext>(this IColumnTypeSyntax<TNext> createTableColumnAsTypeSyntax)
            where TNext : IFluentSyntax
        {
            return createTableColumnAsTypeSyntax.AsCustom("TIMESTAMP");
        }
    }
}