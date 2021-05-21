CREATE TABLE IF NOT EXISTS `word_filter`
(
    `id`          INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `word`        VARCHAR(100)     NULL,
    `deep_filter` TINYINT(1)       NULL     DEFAULT '0',
    `is_global`   TINYINT(1)       NULL     DEFAULT '0',
    `from_id`     VARCHAR(15)      NULL,
    `chat_id`     VARCHAR(20)      NULL,
    `created_at`  TIMESTAMP(0)     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB;
