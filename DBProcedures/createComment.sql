DELIMITER //
CREATE PROCEDURE IF NOT EXISTS createComment (auth_mid INT UNSIGNED, report_id INT UNSIGNED, comment TEXT)
BEGIN
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
END//

DELIMITER ;