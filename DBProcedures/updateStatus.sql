DELIMITER //

CREATE PROCEDURE updateStatus(
    IN auth_mid INT UNSIGNED,
    IN report_id INT UNSIGNED,
    IN new_status TINYINT UNSIGNED,
    IN comment TEXT
)
BEGIN
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
END //

DELIMITER ;