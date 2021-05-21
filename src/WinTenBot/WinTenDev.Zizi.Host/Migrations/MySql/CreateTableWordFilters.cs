using FluentMigrator;
using WinTenDev.Zizi.Host.Extensions;

namespace WinTenDev.Zizi.Host.Migrations.MySql
{
    [Migration(120201028201115)]
    public class CreateTableWordFilters : Migration
    {
        private const string TableName = "word_filter";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("word").AsMySqlVarchar(100).NotNullable()
                .WithColumn("is_global").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("from_id").AsMySqlVarchar(15).NotNullable()
                .WithColumn("chat_id").AsMySqlVarchar(30).NotNullable()
                .WithColumn("created_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}