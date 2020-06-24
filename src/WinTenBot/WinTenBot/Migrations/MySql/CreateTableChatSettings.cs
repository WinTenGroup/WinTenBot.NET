using System;
using FluentMigrator;
using WinTenBot.Extensions;

namespace WinTenBot.Migrations.MySql
{
    [Migration(120200621163214)]
    public class CreateTableChatSettings : Migration
    {
        private const string TableName = "group_settings";

        public override void Up()
        {
            if (Schema.Table(TableName).Exists()) return;

            Create.Table(TableName)
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("chat_id").AsString(20)
                .WithColumn("chat_title").AsMySqlVarchar(150)
                .WithColumn("chat_type").AsString(100)
                .WithColumn("members_count").AsInt64().WithDefaultValue(-1)
                .WithColumn("enable_bot").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_afk_stats").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_anti_malfiles").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_badword_filter").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_fed_cas_ban").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_fed_es2_ban").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_fed_spamwatch").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_find_notes").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_find_tags").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_human_verification").AsBoolean().WithDefaultValue(0)
                .WithColumn("enable_reply_notification").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_restriction").AsBoolean().WithDefaultValue(0)
                .WithColumn("enable_security").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_url_filtering").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_unified_welcome").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_warn_username").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_welcome_message").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_word_filter_global").AsBoolean().WithDefaultValue(1)
                .WithColumn("enable_word_filter_group").AsBoolean().WithDefaultValue(1)
                .WithColumn("last_welcome_message_id").AsString(20).WithDefaultValue(-1)
                .WithColumn("last_tags_message_id").AsString(20).WithDefaultValue(-1)
                .WithColumn("rules_link").AsMySqlMediumText().WithDefaultValue("")
                .WithColumn("rules_text").AsMySqlMediumText().WithDefaultValue("")
                .WithColumn("warning_username_limit").AsInt16().WithDefaultValue(3)
                .WithColumn("welcome_message").AsMySqlMediumText().WithDefaultValue("")
                .WithColumn("welcome_button").AsMySqlText().WithDefaultValue("")
                .WithColumn("welcome_media").AsString(150).WithDefaultValue("")
                .WithColumn("welcome_media_type").AsInt16().WithDefaultValue(-1)
                .WithColumn("created_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("updated_at").AsMySqlTimestamp().WithDefault(SystemMethods.CurrentDateTime)
                .Indexed("chat_id");
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}