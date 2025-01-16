SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

DELIMITER $$
CREATE PROCEDURE `createComment` (`auth_mid` INT UNSIGNED, `report_id` INT UNSIGNED, `comment` TEXT)   BEGIN
    DECLARE report_creator_id INT UNSIGNED;
    DECLARE report_severity TINYINT UNSIGNED;
    
    SELECT r.creator_id, r.severity INTO report_creator_id, report_severity FROM reports r WHERE r.id = report_id;

    IF report_creator_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not found', MYSQL_ERRNO = 7501;
    END IF;

    IF report_severity = 5 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Permission denied', MYSQL_ERRNO = 7503;
    END IF;

    INSERT INTO report_comments (report_id, creator_id, time, comment) VALUES (report_id, auth_mid, UNIX_TIMESTAMP(NOW()), comment);
    SELECT LAST_INSERT_ID();
END$$

CREATE PROCEDURE `deleteReport` (`auth_mid` INT UNSIGNED, `report_id` INT UNSIGNED)   BEGIN
    DECLARE report_creator_id INT UNSIGNED;
    DECLARE modified_by_product_owner TINYINT UNSIGNED;
    
    SELECT r.creator_id, r.modified_by_product_owner INTO report_creator_id, modified_by_product_owner FROM reports r WHERE r.id = report_id;

    IF report_creator_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not found', MYSQL_ERRNO = 7501;
    END IF;

    IF auth_mid != report_creator_id THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Permission denied', MYSQL_ERRNO = 7503;
    END IF;

    IF auth_mid = report_creator_id AND modified_by_product_owner = 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Report creator cannot delete this', MYSQL_ERRNO = 7508;
    END IF;

    DELETE FROM reports
    WHERE id = report_id;
    
    DELETE FROM report_comments
    WHERE report_id = report_id;
END$$

CREATE PROCEDURE `editComment` (`auth_mid` INT UNSIGNED, `comment_id` INT UNSIGNED, `comment` TEXT)   BEGIN
    DECLARE comment_creator_id INT UNSIGNED;
    
    SELECT c.creator_id INTO comment_creator_id FROM report_comments c WHERE c.id = comment_id;

    IF comment_creator_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not found', MYSQL_ERRNO = 7501;
    END IF;

    IF auth_mid != comment_creator_id THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Permission denied', MYSQL_ERRNO = 7503;
    END IF;

    UPDATE report_comments
    SET comment = comment, updated_time = UNIX_TIMESTAMP(NOW())
    WHERE id = comment_id;

END$$

CREATE PROCEDURE `editReport` (`auth_mid` INT UNSIGNED, `report_id` INT UNSIGNED, `title` TEXT, `text` TEXT, `actual` TEXT, `expected` TEXT, `problem_type` TINYINT UNSIGNED)   BEGIN
    DECLARE report_creator_id INT UNSIGNED;
    DECLARE modified_by_product_owner TINYINT UNSIGNED;
    DECLARE old_title TEXT;
    DECLARE old_text TEXT;
    DECLARE old_actual TEXT;
    DECLARE old_expected TEXT;
    DECLARE old_problem_type TINYINT UNSIGNED;
    
    SELECT r.creator_id, r.modified_by_product_owner, r.title, r.text, r.actual, r.expected, r.problem_type INTO report_creator_id, modified_by_product_owner, old_title, old_text, old_actual, old_expected, old_problem_type FROM reports r WHERE r.id = report_id;

    IF report_creator_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not found', MYSQL_ERRNO = 7501;
    END IF;

    IF auth_mid != report_creator_id THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Permission denied', MYSQL_ERRNO = 7503;
    END IF;

    IF auth_mid = report_creator_id AND modified_by_product_owner = 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Report creator cannot edit this', MYSQL_ERRNO = 7507;
    END IF;

    UPDATE reports
    SET title = COALESCE(title, old_title), text = COALESCE(text, old_text), actual = COALESCE(actual, old_actual), expected = COALESCE(expected, old_expected), problem_type = IF(problem_type = 0, old_problem_type, problem_type), updated_time = UNIX_TIMESTAMP(NOW())
    WHERE id = report_id;
END$$

CREATE PROCEDURE `getComments` (`auth_mid` INT UNSIGNED, `report_id` INT UNSIGNED)   BEGIN
    DECLARE report_creator_id INT UNSIGNED;
    DECLARE report_severity TINYINT UNSIGNED;
    DECLARE product_id INT UNSIGNED;
    DECLARE product_owner_id INT UNSIGNED;
    
    -- Проверка существования отчёта
    SELECT r.creator_id, r.severity, r.product_id, p.owner_id
    INTO report_creator_id, report_severity, product_id, product_owner_id
    FROM reports r
    JOIN products p ON r.product_id = p.id
    WHERE r.id = report_id;

    IF report_creator_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not found', MYSQL_ERRNO = 7501;
    END IF;

    IF report_severity = 5 AND (auth_mid != report_creator_id AND auth_mid != product_owner_id) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Access denied', MYSQL_ERRNO = 7502;
    END IF;

    SELECT * FROM report_comments WHERE report_id = report_id;
END$$

DELIMITER $$
CREATE PROCEDURE `getSingleReport` (`auth_mid` INT UNSIGNED, `report_id` INT UNSIGNED)
BEGIN
    DECLARE report_creator_id INT UNSIGNED;
    DECLARE report_severity TINYINT UNSIGNED;
    DECLARE product_id INT UNSIGNED;
    DECLARE product_owner_id INT UNSIGNED;
    
    -- Проверка существования отчёта
    SELECT r.creator_id, r.severity, r.product_id, p.owner_id
    INTO report_creator_id, report_severity, product_id, product_owner_id
    FROM reports r
    JOIN products p ON r.product_id = p.id
    WHERE r.id = report_id;

    IF report_creator_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not found', MYSQL_ERRNO = 7501;
    END IF;

    IF report_severity = 5 AND (auth_mid != report_creator_id AND auth_mid != product_owner_id) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Access denied', MYSQL_ERRNO = 7502;
    END IF;

    -- Приходится во второй раз получать отчёт, ибо нельзя всю строку хранить в переменной, а декларировать дупли столбцов таблицы reports не хочу
    SELECT * FROM reports WHERE id = report_id;
END$$

CREATE PROCEDURE `testerror` (`id` INT)   BEGIN
    IF id = 3 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'This is a test error', MYSQL_ERRNO = 7599;
    END IF;
END$$

CREATE PROCEDURE `updateSeverity` (IN `auth_mid` INT UNSIGNED, IN `report_id` INT UNSIGNED, IN `new_severity` TINYINT UNSIGNED, IN `comment` TEXT)   BEGIN
    DECLARE report_creator_id INT UNSIGNED;
    DECLARE product_owner_id INT UNSIGNED;
    DECLARE modified_by_product_owner TINYINT UNSIGNED;
    DECLARE current_severity TINYINT UNSIGNED;
    DECLARE product_id INT UNSIGNED;
    DECLARE comment_id INT UNSIGNED;

    -- Проверка существования отчёта
    SELECT r.creator_id, r.modified_by_product_owner, r.severity, r.product_id, p.owner_id
    INTO report_creator_id, modified_by_product_owner, current_severity, product_id, product_owner_id
    FROM reports r
    JOIN products p ON r.product_id = p.id
    WHERE r.id = report_id;

    IF report_creator_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not found', MYSQL_ERRNO = 7501;
    END IF;

    -- Проверка прав доступа
    IF auth_mid != report_creator_id AND auth_mid != product_owner_id THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Permission denied', MYSQL_ERRNO = 7503;
    END IF;

    -- Не дать менять автору отчёта менять severity, если владелец продукта уже что-то с ним сделал
    IF auth_mid = report_creator_id AND modified_by_product_owner = 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Report creator cannot edit this', MYSQL_ERRNO = 7507;
    END IF;

    -- Если severity одинаковые
    IF current_severity = new_severity THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Wrong severity', MYSQL_ERRNO = 7506;
    END IF;

    -- Обновление severity отчёта
    IF auth_mid = report_creator_id THEN
        UPDATE reports
        SET severity = new_severity, updated_time = UNIX_TIMESTAMP(NOW())
        WHERE id = report_id;
    ELSEIF auth_mid = product_owner_id THEN
        UPDATE reports
        SET severity = new_severity, updated_time = UNIX_TIMESTAMP(NOW()), modified_by_product_owner = 1
        WHERE id = report_id;
    END IF;

    -- Добавление комментария
    INSERT INTO report_comments (report_id, creator_id, time, new_severity, comment)
    VALUES (report_id, auth_mid, UNIX_TIMESTAMP(NOW()), new_severity, comment);

    SET comment_id = LAST_INSERT_ID();

    -- Возврат ID комментария
    SELECT comment_id;
END$$

CREATE PROCEDURE `updateStatus` (IN `auth_mid` INT UNSIGNED, IN `report_id` INT UNSIGNED, IN `new_status` TINYINT UNSIGNED, IN `comment` TEXT)   BEGIN
    DECLARE report_creator_id INT UNSIGNED;
    DECLARE product_owner_id INT UNSIGNED;
    DECLARE modified_by_product_owner TINYINT UNSIGNED;
    DECLARE current_status TINYINT UNSIGNED;
    DECLARE product_id INT UNSIGNED;
    DECLARE comment_id INT UNSIGNED;

    -- Проверка существования отчёта
    SELECT r.creator_id, r.modified_by_product_owner, r.status, r.product_id, p.owner_id
    INTO report_creator_id, modified_by_product_owner, current_status, product_id, product_owner_id
    FROM reports r
    JOIN products p ON r.product_id = p.id
    WHERE r.id = report_id;

    IF report_creator_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Not found', MYSQL_ERRNO = 7501;
    END IF;

    -- Проверка прав доступа
    IF auth_mid != report_creator_id AND auth_mid != product_owner_id THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Permission denied', MYSQL_ERRNO = 7503;
    END IF;

    -- Не дать менять автору отчёта менять статус, если владелец продукта уже что-то с ним сделал
    IF auth_mid = report_creator_id AND modified_by_product_owner = 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Report creator cannot edit this', MYSQL_ERRNO = 7507;
    END IF;

    -- Если статусы одинаковые
    IF current_status = new_status THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Wrong status', MYSQL_ERRNO = 7504;
    END IF;


    -- Если пользователь является владельцем продукта
    IF auth_mid = product_owner_id THEN
        IF current_status = 0 AND new_status = 7 THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Wrong status', MYSQL_ERRNO = 7504;
        END IF;

        IF (new_status = 8 OR new_status = 10) AND (comment IS NULL OR CHAR_LENGTH(comment) = 0) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Comment required for this status', MYSQL_ERRNO = 7505;
        END IF;
    END IF;

    -- Если пользователь является автором отчёта
    IF auth_mid = report_creator_id THEN
        IF NOT ((current_status IN (8, 10) AND new_status = 7) OR
                (current_status = 11 AND new_status IN (12, 7))) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Wrong status', MYSQL_ERRNO = 7504;
        END IF;
    END IF;

    -- Обновление статуса отчёта
    IF auth_mid = report_creator_id THEN
        UPDATE reports
        SET status = new_status, updated_time = UNIX_TIMESTAMP(NOW())
        WHERE id = report_id;
    ELSEIF auth_mid = product_owner_id THEN
        UPDATE reports
        SET status = new_status, updated_time = UNIX_TIMESTAMP(NOW()), modified_by_product_owner = 1
        WHERE id = report_id;
    END IF;

    -- Добавление комментария
    INSERT INTO report_comments (report_id, creator_id, time, new_status, comment)
    VALUES (report_id, auth_mid, UNIX_TIMESTAMP(NOW()), new_status, comment);

    SET comment_id = LAST_INSERT_ID();

    -- Возврат ID комментария
    SELECT comment_id;
END$$

DELIMITER ;

CREATE TABLE `invites` (
  `id` int UNSIGNED NOT NULL,
  `code` varchar(32) NOT NULL,
  `user_name` varchar(32) NOT NULL,
  `creation_time` bigint NOT NULL,
  `created_by` int UNSIGNED NOT NULL,
  `invited_member_id` int UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

CREATE TABLE `members` (
  `id` int UNSIGNED NOT NULL,
  `user_name` varchar(32) DEFAULT NULL,
  `first_name` varchar(50) DEFAULT NULL,
  `last_name` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

CREATE TABLE `member_credentials` (
  `member_id` int UNSIGNED NOT NULL,
  `password_hash` varchar(64) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

CREATE TABLE `products` (
  `id` int UNSIGNED NOT NULL,
  `owner_id` int UNSIGNED NOT NULL,
  `name` varchar(64) NOT NULL,
  `is_finished` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `reports` (
  `id` int UNSIGNED NOT NULL,
  `product_id` int UNSIGNED NOT NULL,
  `creator_id` int UNSIGNED NOT NULL,
  `time` bigint NOT NULL,
  `title` varchar(128) NOT NULL,
  `text` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `actual` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `expected` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `severity` tinyint UNSIGNED NOT NULL,
  `problem_type` tinyint UNSIGNED NOT NULL,
  `status` tinyint UNSIGNED NOT NULL DEFAULT '0',
  `modified_by_product_owner` tinyint(1) NOT NULL DEFAULT '0',
  `updated_time` bigint DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `report_comments` (
  `id` int UNSIGNED NOT NULL,
  `report_id` int UNSIGNED NOT NULL,
  `creator_id` int UNSIGNED NOT NULL,
  `time` bigint NOT NULL,
  `updated_time` bigint DEFAULT NULL,
  `new_severity` tinyint UNSIGNED DEFAULT NULL,
  `new_status` tinyint UNSIGNED DEFAULT NULL,
  `comment` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


ALTER TABLE `invites`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `code` (`code`);

ALTER TABLE `members`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `id_UNIQUE` (`id`);

ALTER TABLE `member_credentials`
  ADD UNIQUE KEY `member_id` (`member_id`);

ALTER TABLE `products`
  ADD PRIMARY KEY (`id`);

ALTER TABLE `reports`
  ADD PRIMARY KEY (`id`);

ALTER TABLE `report_comments`
  ADD PRIMARY KEY (`id`);


ALTER TABLE `invites`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE `members`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE `products`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE `reports`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

ALTER TABLE `report_comments`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;