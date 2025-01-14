DELIMITER //
CREATE PROCEDURE IF NOT EXISTS deleteReport (auth_mid INT UNSIGNED, report_id INT UNSIGNED)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM reports WHERE id = report_id) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Report with the specified ID does not exist.', MYSQL_ERRNO = 7501;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM reports WHERE id = report_id AND creator_id = auth_mid) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'You are not the creator of this report.', MYSQL_ERRNO = 7503;
    END IF;

    DELETE FROM reports
    WHERE id = report_id;
    
    DELETE FROM report_comments
    WHERE report_id = report_id;
END//

DELIMITER ;