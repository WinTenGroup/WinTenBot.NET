CREATE TABLE IF NOT EXISTS `stikers`
(
    `id`          INT(10)      NOT NULL AUTO_INCREMENT,
    `nama`        VARCHAR(100) NULL     DEFAULT 'nn',
    `file_id`     VARCHAR(50)  NULL,
    `id_telegram` VARCHAR(20)  NULL,
    `id_grup`     VARCHAR(30)  NULL,
    `created_at`  TIMESTAMP(0) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at`  TIMESTAMP(0) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB;
