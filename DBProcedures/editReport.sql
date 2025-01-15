DELIMITER //
CREATE PROCEDURE IF NOT EXISTS editReport (auth_mid INT UNSIGNED, report_id INT UNSIGNED, title TEXT, text TEXT, actual TEXT, expected TEXT, problem_type TINYINT UNSIGNED)
BEGIN
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
END//

DELIMITER ;