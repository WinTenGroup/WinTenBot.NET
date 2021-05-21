CREATE TABLE IF NOT EXISTS `hit_activity`
(
    `id`              INT(11)      NOT NULL AUTO_INCREMENT,
    `via_bot`         VARCHAR(50)  NULL,
    `message_type`    VARCHAR(50)  NULL,
    `from_id`         VARCHAR(50)  NULL,
    `from_first_name` MEDIUMTEXT   NULL,
    `from_last_name`  MEDIUMTEXT   NULL,
    `from_username`   VARCHAR(50)  NULL,
    `from_lang_code`  VARCHAR(50)  NULL,
    `chat_id`         VARCHAR(30)  NULL,
    `chat_username`   VARCHAR(50)  NULL,
    `chat_type`       VARCHAR(20)  NULL,
    `chat_title`      VARCHAR(50)  NULL,
    `timestamp`       TIMESTAMP(0) NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`) USING BTREE,
    INDEX `index` (`from_id`(20), `chat_id`(20)) USING BTREE
) ENGINE = InnoDB;
