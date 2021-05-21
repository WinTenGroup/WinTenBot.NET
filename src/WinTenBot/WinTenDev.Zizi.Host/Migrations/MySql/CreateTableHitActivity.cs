using FluentMigrator;
using WinTenDev.Zizi.Host.Extensions;

namespace WinTenDev.Zizi.Host.Migrations.MySql
{
    [Migration(1202008151541)]
    public class CreateTableHitActivity : Migration
    {
        private const string TableName = "hit_activity";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt16().Identity().PrimaryKey()
                .WithColumn("via_bot").AsMySqlVarchar(128).Nullable()
                .WithColumn("message_type").AsMySqlVarchar(100).Nullable()
                .WithColumn("from_id").AsInt32()
                .WithColumn("from_first_name").AsMySqlText().Nullable()
                .WithColumn("from_last_name").AsMySqlText().Nullable()
                .WithColumn("from_username").AsMySqlVarchar(128).Nullable()
                .WithColumn("from_lang_code").AsMySqlVarchar(20)
                .WithColumn("chat_id").AsInt64()
                .WithColumn("chat_username").AsMySqlVarchar(128).Nullable()
                .WithColumn("chat_type").AsMySqlVarchar(25)
                .WithColumn("chat_title").AsMySqlVarchar(128).Nullable()
                .WithColumn("timestamp").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}