using FluentMigrator;
using WinTenDev.Zizi.Host.Extensions;

namespace WinTenDev.Zizi.Host.Migrations.MySql
{
    [Migration(120200603201728)]
    public class CreateTableTags : Migration
    {
        private const string TableName = "tags";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("tag").AsMySqlVarchar(100)
                .WithColumn("content").AsMySqlText()
                .WithColumn("btn_data").AsMySqlText()
                .WithColumn("type_data").AsMySqlVarchar(10).WithDefaultValue(-1)
                .WithColumn("file_id").AsMySqlVarchar(200)
                .WithColumn("from_id").AsInt32()
                .WithColumn("chat_id").AsInt64()
                .WithColumn("created_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}