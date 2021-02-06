using System;
using System.Reflection;
using FluentMigrator.Builders;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Extensions
{
    public static class FluentMigrationExtension
    {
        public static IServiceCollection AddFluentMigration(this IServiceCollection services)
        {
            var appConfig = services.BuildServiceProvider().GetRequiredService<AppConfig>();
            var connStr = appConfig.ConnectionStrings.MySql;

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
                .AddLogging(lb => lb.AddSerilog());

            return services;
        }

        public static IApplicationBuilder UseFluentMigration(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;
            var scopes = services.CreateScope();
            var appConfig = services.GetRequiredService<AppConfig>();
            var runner = scopes.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // if (appConfig == null) return app;
            // if (runner == null) return app;

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