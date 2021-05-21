CREATE TABLE IF NOT EXISTS `fbans_admin`
(
    `id`            INT(11)      NOT NULL AUTO_INCREMENT,
    `user_id`       VARCHAR(20)  NOT NULL DEFAULT '0',
    `username`      VARCHAR(130) NULL,
    `promoted_by`   VARCHAR(20)  NULL,
    `promoted_from` VARCHAR(20)  NULL     DEFAULT '0',
    `is_banned`     TINYINT(1)   NULL     DEFAULT '0',
    `created_at`    TIMESTAMP(0) NULL     DEFAULT CURRENT_TIMESTAMP,
    `updated_at`    TIMESTAMP(0) NULL     DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB;
