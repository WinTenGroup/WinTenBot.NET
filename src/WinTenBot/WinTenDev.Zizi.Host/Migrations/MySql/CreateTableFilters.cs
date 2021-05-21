using FluentMigrator;
using WinTenDev.Zizi.Host.Extensions;

namespace WinTenDev.Zizi.Host.Migrations.MySql
{
    [Migration(120201028233957)]
    public class CreateTableFilters : Migration
    {
        private const string TableName = "filters";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt64().Identity().PrimaryKey()
                .WithColumn("slug").AsMySqlVarchar(75).NotNullable()
                .WithColumn("content").AsMySqlText()
                .WithColumn("btn_data").AsMySqlText()
                .WithColumn("from_id").AsInt32().NotNullable()
                .WithColumn("chat_id").AsInt64().NotNullable()
                .WithColumn("created_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}