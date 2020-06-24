using System;
using FluentMigrator.Builders;
using FluentMigrator.Infrastructure;

namespace WinTenBot.Extensions
{
    public static class FluentMigrationExtension
    {
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