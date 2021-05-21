using FluentMigrator;
using WinTenDev.Zizi.Host.Extensions;

namespace WinTenDev.Zizi.Host.Migrations.MySql
{
    [Migration(120200603195441)]
    public class CreateTableGlobalBanAdmin:Migration
    {
        private const string TableName = "gban_admin";
        
        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt32()
                .WithColumn("username").AsMySqlVarchar(128)
                .WithColumn("promoted_by").AsInt32()
                .WithColumn("promoted_from").AsInt64()
                .WithColumn("is_banned").AsBoolean()
                .WithColumn("created_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}