DELIMITER //
CREATE PROCEDURE IF NOT EXISTS editComment (auth_mid INT UNSIGNED, comment_id INT UNSIGNED, comment TEXT)
BEGIN
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

END//

DELIMITER ;