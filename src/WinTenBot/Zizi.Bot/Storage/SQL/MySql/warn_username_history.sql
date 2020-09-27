CREATE TABLE IF NOT EXISTS `warn_username_history` (
	`id` INT(11) NOT NULL AUTO_INCREMENT,
	`chat_id` BIGINT(30) NULL DEFAULT NULL,
	`from_id` BIGINT(20) NULL DEFAULT NULL,
	`first_name` TINYTEXT NULL DEFAULT NULL,
	`last_name` TINYTEXT NULL DEFAULT NULL,
	`step_count` INT(11) NULL DEFAULT NULL,
	`last_warn_message_id` INT(11) NULL DEFAULT -1,
	`created_at` DATETIME NULL DEFAULT current_timestamp(),
	`updated_at` DATETIME NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
	PRIMARY KEY (`id`) USING BTREE
)ENGINE=InnoDB;
