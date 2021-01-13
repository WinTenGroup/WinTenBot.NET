using FluentMigrator;
using Zizi.Bot.Extensions;

namespace Zizi.Bot.Migrations.MySql
{
    [Migration(120210113001)]
    public class CreateTableWarnUsernameHistory: Migration
    {
        private const string TableName = "warn_username_history";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("chat_id").AsInt64()
                .WithColumn("from_id").AsInt32()
                .WithColumn("first_name").AsMySqlText()
                .WithColumn("last_name").AsMySqlText()
                .WithColumn("step_count").AsMySqlText()
                .WithColumn("last_warn_message_id").AsInt64()
                .WithColumn("created_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("updated_at").AsMySqlTimestamp().WithDefaultValue("ON UPDATE current_timestamp()");

        }


        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}