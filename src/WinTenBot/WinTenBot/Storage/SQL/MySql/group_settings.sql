﻿CREATE TABLE IF NOT EXISTS `group_settings`
(
    `id`                               INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `chat_id`                          VARCHAR(20)      NOT NULL,
    `chat_title`                       VARCHAR(100)     NULL     DEFAULT '',
    `chat_type`                        VARCHAR(100)     NULL,
    `is_admin`                         TINYINT(1)       NULL     DEFAULT '0',
    `enable_bot`                       TINYINT(1)       NOT NULL DEFAULT '1',
    `enable_anti_malfiles`             TINYINT(1)       NULL     DEFAULT '1',
    `enable_badword_filter`            TINYINT(1)       NULL     DEFAULT '1',
    `enable_url_filtering`             TINYINT(1)       NOT NULL DEFAULT '1',
    `enable_human_verification`        TINYINT(1)       NOT NULL DEFAULT '0',
    `enable_federation_ban`            TINYINT(1)       NOT NULL DEFAULT '1',
    `enable_reply_notification`        TINYINT(1)       NOT NULL DEFAULT '1',
    `enable_restriction`               TINYINT(1)       NULL     DEFAULT '0',
    `enable_security`                  TINYINT(1)       NULL     DEFAULT '1',
    `enable_unified_welcome`           TINYINT(1)       NULL     DEFAULT '1',
    `enable_warn_username`             TINYINT(1)       NULL     DEFAULT '1',
    `enable_welcome_message`           TINYINT(1)       NULL     DEFAULT '1',
    `last_welcome_message_id`          VARCHAR(20)      NULL     DEFAULT -1,
    `last_tags_message_id`             VARCHAR(20)      NULL     DEFAULT -1,
    `last_setting_message_id`          VARCHAR(20)      NULL     DEFAULT -1,
    `last_warning_username_message_id` VARCHAR(20)      NULL     DEFAULT -1,
    `enable_word_filter_per_group`     TINYINT(1)       NULL     DEFAULT '1',
    `enable_word_filter_group_wide`    TINYINT(1)       NULL     DEFAULT '1',
    `rules_link`                       TEXT(65535)      NULL,
    `rules_text`                       TEXT(65535)      NULL,
    `warning_username_limit`           INT(20)          NULL     DEFAULT '7',
    `welcome_message`                  TEXT(65535)      NULL,
    `welcome_button`                   TEXT(65535)      NULL,
    `welcome_media`                    VARCHAR(128)     NULL,
    `welcome_media_type`               VARCHAR(20)      NULL     DEFAULT -1,
    `members_count`                    INT(11)          NULL     DEFAULT -1,
    `created_at`                       TIMESTAMP(0)     NULL     DEFAULT CURRENT_TIMESTAMP,
    `updated_at`                       TIMESTAMP(0)     NULL     DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`) USING BTREE,
    INDEX `Index 2` (`chat_id`) USING BTREE
) ENGINE = InnoDB;
