DELIMITER //
CREATE PROCEDURE IF NOT EXISTS getSingleReport (auth_mid INT UNSIGNED, report_id INT UNSIGNED)
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
END//

DELIMITER ;