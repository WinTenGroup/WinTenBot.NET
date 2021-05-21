using FluentMigrator;
using WinTenDev.Zizi.Host.Extensions;

namespace WinTenDev.Zizi.Host.Migrations.MySql
{
    [Migration(120200603195322)]
    public class CreateTableSpells : Migration
    {
        private const string TableName = "spells";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("typo").AsMySqlVarchar(100)
                .WithColumn("fix").AsMySqlVarchar(100)
                .WithColumn("from_id").AsInt32()
                .WithColumn("chat_id").AsInt16()
                .WithColumn("created_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}