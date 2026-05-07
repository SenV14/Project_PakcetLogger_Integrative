-- SQL setup for Project_PakcetLogger_Integrative
-- Run this in MySQL Workbench against the `packetlogger_login` database.
-- Creates a minimal table used by the current C# code and an optional recommended tokens table.

-- 1) Minimal table required by current code (packet_logger_authentication.token_enc)
CREATE TABLE IF NOT EXISTS `packet_logger_authentication` (
  `email_info` VARCHAR(255) NOT NULL,
  `token_enc` LONGTEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY `ux_email_info` (`email_info`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2) Optional recommended table for multiple providers (google, github) with refresh tokens
-- Use this instead of (or in addition to) the minimal table if you need refresh tokens or providers.

CREATE TABLE IF NOT EXISTS `user_tokens` (
  `id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `email_info` VARCHAR(255) NULL,
  `provider` VARCHAR(50) NOT NULL,
  `provider_user_id` VARCHAR(255) NULL,
  `access_token_enc` LONGTEXT NOT NULL,
  `refresh_token_enc` LONGTEXT NULL,
  `id_token_enc` LONGTEXT NULL,
  `scopes` TEXT NULL,
  `expires_at` DATETIME NULL,
  `revoked` TINYINT(1) NOT NULL DEFAULT 0,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY `ux_provider_user` (`provider`,`provider_user_id`),
  KEY `ix_email_provider` (`email_info`,`provider`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3) Helpful ALTER statements (if you already have a table with different name/columns)
-- Example: add token_enc to an existing table
-- ALTER TABLE `packet_authentication_t` ADD COLUMN IF NOT EXISTS `token_enc` LONGTEXT NULL;
-- ALTER TABLE `packet_authentication_t` ADD UNIQUE KEY `ux_email_info` (`email_info`);

-- Notes:
-- - The C# code currently performs an INSERT ... ON DUPLICATE KEY UPDATE on `packet_logger_authentication` using `email_info` as the unique key.
-- - If you prefer the `user_tokens` approach, update the C# SaveEncryptedToken method to upsert into `user_tokens` (provider-aware).
-- - Use LONGTEXT for base64 DPAPI blobs.
