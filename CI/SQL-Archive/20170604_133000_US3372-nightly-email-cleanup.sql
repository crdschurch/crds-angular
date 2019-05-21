USE [MinistryPlatform]
GO

CREATE INDEX IX_dp_Communication_Messages__CommunicationID ON dp_Communication_Messages(Communication_ID);
GO


-- 06/04/2017
-- This procedure is a band-aid to clean up unsent emails that are linked with trip donations.
-- When a user donates to another user's trip, the donor may optionally provide a message
-- that will be delivered via email to the recipient once the donation is fully processed.
-- The process is currently implemented to work as follows:
--
-- 1) A donor submits their donation via the website
-- 2) The backend code calls stripe to process the donation, adds a donation to the database
--    with a status of "pending", and adds an email message from the donor to the recipient
--    and sets the communication status to "draft" so that the email remains unsent
--    [DonorRepository.AddDonationCommunication()]
-- 3) At a future point, stripe sends a callback (webhook) to update the status of the donation
--    to declined/succeeded/deposited.
-- 4) If the status from stripe is "succeeded", the backend code changes the status of the existing
--    email from "Draft" to "Ready To Send" so that the email is queued to send
--    [DonorRepository.FinishSendMessageFromDonor()]
--
-- There are a couple issues with the current process.  First, sometimes stripe sends the webhook
-- too soon before the donation and email have been added to the database (to fix this, we really
-- need to add the donation to the database BEFORE calling stripe, but that's on the backlog for
-- another day).  If the webhook happens too soon, neither the donation or email exists in the
-- database, so we cannot queue the email for delivery at that point.  As a result, the email that
-- is created later is never sent (i.e., remains stuck in draft status forever).  Second, the backend
-- code is currently only updating the draft emails when it receives a "succeeded" status from stripe.
-- If stripe sends a "declined" status, the email remains stuck in draft status forever.
--
-- Emails that are linked with declined donations should never be sent, so we'll simply delete those.
-- Ideally deleting emails for declined donations should be done in the backend code
-- (e.g., FinishSendMessageFromDonor()).  However, the current timing issues with stripe statuses
-- mentioned above makes this unreliable, so running this stored proc once daily is the workaround.
-- If the stripe timing issues are corrected in the future, this stored proc can be deleted and the
-- logic to delete emails for "declined" donations should be moved to the backend code.

CREATE PROCEDURE [dbo].[crds_service_clean_donation_emails_nightly]
AS
BEGIN
    SET NOCOUNT ON

    -- Part 1: Send emails that are "stuck" and should have been sent already

    -- Don't send emails that are older than 30 days
    DECLARE @minDate DATE = DATEADD(d, -30, GETDATE())

    DECLARE @idsToSend TABLE (
        Donation_Communications_Id INT NOT NULL,
        Communication_Id INT NOT NULL
    );

    -- Find donations that are Succeeded or Deposited that have unsent email (Draft status)
    INSERT INTO @idsToSend
        (Donation_Communications_Id, Communication_Id)
    SELECT
        dc.Donation_Communications_Id,
        c.Communication_Id
    FROM
        cr_Donation_Communications dc
        INNER JOIN dp_Communications c ON c.Communication_Id = dc.Communication_Id
        INNER JOIN Donations d ON d.Donation_Id = dc.Donation_Id
    WHERE
        d.Donation_Status_ID IN (2, 4) AND      -- 2 = Deposited, 4 = Succeeded
        c.Communication_Status_ID = 1 AND       -- 1 = Draft
        c.Template = 0 AND
        c.Start_Date >= @minDate
    ;

    DECLARE @numToSend INT;
    SELECT @numToSend = COUNT(*) FROM @idsToSend;
    PRINT 'Found ' + CONVERT(VARCHAR(20), @numToSend) + ' emails that should be sent.';

    -- Send those emails
    UPDATE c
    SET c.Communication_Status_ID = 3           -- 3 = Ready To Send
    FROM
        dp_Communications c
    WHERE
        c.Communication_Id IN (SELECT Communication_Id FROM @idsToSend)
    ;

    -- Remove the emails we just sent from the list of unsent email
    DELETE FROM cr_Donation_Communications
    WHERE Donation_Communications_Id IN (SELECT Donation_Communications_Id FROM @idsToSend)
    ;

    -- Part 2: Delete unsent emails that are associated with donations that were Declined.

    DECLARE @idsToDelete TABLE (
        Donation_Communications_Id INT NOT NULL,
        Communication_Id INT NOT NULL
    );

    INSERT INTO @idsToDelete
        (Donation_Communications_Id, Communication_Id)
    SELECT
        dc.Donation_Communications_Id,
        c.Communication_Id
    FROM
        cr_Donation_Communications dc
        INNER JOIN dp_Communications c ON c.Communication_Id = dc.Communication_Id
        INNER JOIN Donations d ON d.Donation_Id = dc.Donation_Id
    WHERE
        d.Donation_Status_ID = 3 AND            -- 3 = Declined
        c.Communication_Status_ID = 1 AND       -- 1 = Draft
        c.Template = 0
    ;

    DECLARE @numToDelete INT;
    SELECT @numToDelete = COUNT(*) FROM @idsToDelete;
    PRINT 'Found ' + CONVERT(VARCHAR(20), @numToDelete) + ' emails that should be deleted.';

    BEGIN TRY
        BEGIN TRAN

        DELETE FROM cr_Donation_Communications
        WHERE Donation_Communications_Id IN (SELECT Donation_Communications_Id FROM @idsToDelete)
        ;

        DELETE FROM dp_Communication_Messages
        WHERE Communication_ID IN (SELECT Communication_Id FROM @idsToDelete)
        ;

        DELETE FROM dp_Communications
        WHERE Communication_ID IN (SELECT Communication_Id FROM @idsToDelete)
        ;

        COMMIT
        PRINT 'crds_service_clean_donation_emails_nightly completed successfully.'
    END TRY

    BEGIN CATCH
        PRINT 'crds_service_clean_donation_emails_nightly failed: ' + ERROR_MESSAGE()
        IF @@TRANCOUNT > 0
            ROLLBACK
    END CATCH
END
GO


ALTER PROCEDURE [dbo].[service_church_specific]
    @DomainID INT
AS
BEGIN
    EXEC crds_service_assign_pledges_nightly @DomainID
    EXEC crds_service_clean_donation_emails_nightly
    EXEC crds_service_update_email_nightly
END
GO
