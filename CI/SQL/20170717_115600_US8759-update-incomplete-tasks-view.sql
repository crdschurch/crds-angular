USE [MinistryPlatform]
GO

-- Modify the View Clause for "My Incomplete Tasks - event details" to exclude room reservations
-- where the underlying event has already occurred.  Note that this view shows other tasks in
-- addition to room reservations, so don't mess with tasks unrelated to room reservations.
UPDATE dp_Page_Views
SET View_Clause = 'dp_Tasks.Assigned_User_ID = dp_UserID 
AND dp_Tasks.Completed = 0
AND NOT EXISTS (
    SELECT
        1
    FROM
        dp_Tasks t
        INNER JOIN Event_Rooms er ON t._Record_ID = er.Event_Room_ID AND t._Page_ID = 384
        INNER JOIN Events e ON e.Event_ID = er.Event_ID
    WHERE
        t.Task_ID = dp_Tasks.Task_ID AND
        e.Event_End_Date < GETDATE()
)'
WHERE Page_View_ID = 93287
;

GO
