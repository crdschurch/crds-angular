USE [MinistryPlatform]
GO

-- Room reservation requests appear in a page view that Facilities uses for approval / rejection.
-- If Facilities does not approve/reject the request before the event has occurred, the task will
-- stay in an incomplete state forever.  This procedure will look for "expired" room reservation
-- requests that did not get approved/rejected and then quietly mark those as a completed (without
-- sending email to the requester, etc.)
CREATE PROCEDURE [dbo].[crds_service_clean_room_reservations_nightly]
AS
BEGIN
    UPDATE t
    SET t.Completed = 1
    FROM
        dp_Tasks t
        INNER JOIN Event_Rooms AS er ON er.Event_Room_ID = t._Record_ID AND t._Page_ID = 384
        INNER JOIN Events AS e ON e.Event_ID = er.Event_ID
    WHERE
        t.Completed = 0 AND
        t._Rejected = 0 AND
        e.Event_End_Date < GETDATE()
    ;

    PRINT 'crds_service_clean_room_reservations_nightly: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' tasks were completed.'
END
GO


ALTER PROCEDURE [dbo].[service_church_specific]
    @DomainID INT
AS
BEGIN
    EXEC crds_service_assign_pledges_nightly @DomainID
    EXEC crds_service_clean_room_reservations_nightly
    EXEC crds_service_clean_donation_emails_nightly
    EXEC crds_service_update_email_nightly
END
GO
