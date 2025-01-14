CREATE PROCEDURE reportRemover (IN report_id INT);
BEGIN;
    IF NOT EXISTS (SELECT 1 FROM reports WHERE id = @report_id) THEN
        SIGNAL SQLSTATE '75001'
        SET MESSAGE_TEXT = 'Report with the specified ID does not exist';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM reports WHERE id = @report_id AND creator_id = @authorized_user_id) THEN
        SIGNAL SQLSTATE '75002'
        SET MESSAGE_TEXT = 'Unauthorized action. You are not the creator of this report';
    END IF;

    DELETE FROM reports
    WHERE id = @report_id
      AND creator_id = @authorized_user_id;
END;