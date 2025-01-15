DELIMITER //
CREATE PROCEDURE IF NOT EXISTS deleteReport (auth_mid INT UNSIGNED, report_id INT UNSIGNED)
BEGIN
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
END//

DELIMITER ;