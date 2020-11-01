using FluentMigrator;
using Zizi.Bot.Extensions;

namespace Zizi.Bot.Migrations.MySql
{
    [Migration(120201028113701)]
    public class CreateTableHumanVerification : Migration
    {
        private const string TableName = "human_verification";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("chat_id").AsInt64().NotNullable()
                .WithColumn("from_id").AsInt32().NotNullable()
                .WithColumn("created_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}