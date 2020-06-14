CREATE TABLE IF NOT EXISTS `tags`
(
    `id`         INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
    `id_chat`    VARCHAR(20)      NOT NULL,
    `id_user`    VARCHAR(10)      NOT NULL,
    `tag`        VARCHAR(100)     NOT NULL,
    `content`    TEXT(65535)      NOT NULL,
    `btn_data`   TEXT(65535)      NULL,
    `type_data`  VARCHAR(10)      NOT NULL DEFAULT 'text',
    `id_data`    VARCHAR(105)     NULL,
    `created_at` TIMESTAMP(0)     NULL     DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP(0)     NULL     DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB;
