using FluentMigrator;
using Zizi.Bot.Extensions;

namespace Zizi.Bot.Migrations.MySql
{
    [Migration(202003141720)]
    public class CreateTableRssSettings : Migration
    {
        private const string TableName = "rss_settings";
        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;
            
            Create.Table(TableName)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("from_id").AsInt32().NotNullable()
                .WithColumn("chat_id").AsInt64().NotNullable()
                .WithColumn("url_feed").AsMySqlText().NotNullable()
                .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}