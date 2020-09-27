using FluentMigrator;
using Zizi.Bot.Extensions;

namespace Zizi.Bot.Migrations.MySql
{
    [Migration(120200603220344)]
    public class CreateTableGlobalBan:Migration
    {
        private const string TableName = "global_bans";
        
        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt32()
                .WithColumn("reason").AsMySqlText()
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