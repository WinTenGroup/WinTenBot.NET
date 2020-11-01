CREATE TABLE IF NOT EXISTS `notes`
(
    `id_note`    INT(11)      NOT NULL AUTO_INCREMENT,
    `slug`       VARCHAR(75)  NOT NULL,
    `link`       MEDIUMTEXT   NOT NULL,
    `content`    TEXT(65535)  NOT NULL,
    `user_id`    INT(11)      NULL,
    `chat_id`    VARCHAR(30)  NULL,
    `author`     VARCHAR(50)  NOT NULL,
    `created_at` TIMESTAMP(0) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP(0) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `btn_data`   TEXT(65535)  NULL,
    PRIMARY KEY (`id_note`) USING BTREE
) ENGINE = InnoDB;
