using FluentMigrator;
using Zizi.Bot.Extensions;

namespace Zizi.Bot.Migrations.MySql
{
    [Migration(20200530065305)]
    public class CreateTableWordsLearning:Migration
    {
        private const string TableName = "words_learning";
        
        public override void Up()
        {
            if(Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("label").AsMySqlVarchar(20)
                .WithColumn("message").AsMySqlText()
                .WithColumn("from_id").AsInt32()
                .WithColumn("chat_id").AsInt64()
                .WithColumn("timestamp").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}