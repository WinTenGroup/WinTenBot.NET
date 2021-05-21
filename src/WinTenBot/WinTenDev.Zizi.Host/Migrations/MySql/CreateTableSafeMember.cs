using FluentMigrator;
using WinTenDev.Zizi.Host.Extensions;

namespace WinTenDev.Zizi.Host.Migrations.MySql
{
    [Migration(120200711232301)]
    public class CreateTableSafeMember : Migration
    {
        private const string TableName = "SafeMembers";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("UserId").AsInt32()
                .WithColumn("SafeStep").AsInt16()
                .WithColumn("CreatedAt").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}