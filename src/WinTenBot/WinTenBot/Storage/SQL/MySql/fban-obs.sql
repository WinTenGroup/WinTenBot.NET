CREATE TABLE IF NOT EXISTS `fbans`
(
    `id`          INT(11)      NOT NULL AUTO_INCREMENT,
    `user_id`     VARCHAR(20)  NOT NULL DEFAULT '0',
    `reason_ban`  MEDIUMTEXT   NULL,
    `banned_by`   VARCHAR(20)  NOT NULL DEFAULT 'Manual',
    `banned_from` VARCHAR(25)  NOT NULL DEFAULT 'Importer',
    `created_at`  TIMESTAMP(0) NULL     DEFAULT CURRENT_TIMESTAMP,
    `updated_at`  TIMESTAMP(0) NULL     DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`, `user_id`) USING BTREE
) ENGINE = InnoDB;
