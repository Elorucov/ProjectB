DELIMITER //

CREATE PROCEDURE updateSeverity(
    IN auth_mid INT UNSIGNED,
    IN report_id INT UNSIGNED,
    IN new_severity TINYINT UNSIGNED,
    IN comment TEXT
)
BEGIN
    DECLARE report_creator_id INT UNSIGNED;
    DECLARE product_owner_id INT UNSIGNED;
    DECLARE severity_changed_by_owner_once TINYINT(1) UNSIGNED;
    DECLARE current_severity TINYINT UNSIGNED;
    DECLARE product_id INT UNSIGNED;
    DECLARE comment_id INT UNSIGNED;

    -- Проверка существования отчёта
    SELECT r.creator_id, r.severity_changed_by_owner_once, r.severity, r.product_id, p.owner_id
    INTO report_creator_id, severity_changed_by_owner_once, current_severity, product_id, product_owner_id
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

    -- Не дать менять автору отчёта менять severity, если отчёт уже имеет другой статус
    IF auth_mid = report_creator_id AND severity_changed_by_owner_once = 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Report creator cannot change severity after product owner has already changed severity', MYSQL_ERRNO = 7506;
    END IF;

    -- Если severity одинаковые
    IF current_severity = new_severity THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Wrong severity', MYSQL_ERRNO = 7507;
    END IF;

    -- Обновление severity отчёта
    IF auth_mid = report_creator_id THEN
        UPDATE reports
        SET severity = new_severity
        WHERE id = report_id;
    ELSEIF auth_mid = product_owner_id THEN
        UPDATE reports
        SET severity = new_severity, severity_changed_by_owner_once = 1
        WHERE id = report_id;
    END IF;

    -- Добавление комментария
    INSERT INTO report_comments (report_id, creator_id, time, new_severity, comment)
    VALUES (report_id, auth_mid, UNIX_TIMESTAMP(NOW()), new_severity, comment);

    SET comment_id = LAST_INSERT_ID();

    -- Возврат ID комментария
    SELECT comment_id;
END //

DELIMITER ;