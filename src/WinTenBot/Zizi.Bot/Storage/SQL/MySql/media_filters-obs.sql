CREATE TABLE IF NOT EXISTS `media_filters`
(
    `id`           INT(11)      NOT NULL AUTO_INCREMENT,
    `file_id`      VARCHAR(50)  NOT NULL ,
    `type_data`    VARCHAR(30)  NOT NULL ,
    `blocked_by`   VARCHAR(20)  NOT NULL,
    `blocked_from` VARCHAR(20)  NOT NULL ,
    `created_at`   TIMESTAMP(0) NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`   TIMESTAMP(0) NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`, `file_id`) USING BTREE
)ENGINE = InnoDB;
