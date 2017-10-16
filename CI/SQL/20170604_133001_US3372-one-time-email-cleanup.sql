USE [MinistryPlatform]
GO

-- This is a one-time clean up of emails linked to trip donations that were never sent.
-- These should have been sent, but some are over a year old now, so sending them at
-- this point would confuse people.  Therefore, we're going to simply delete trip
-- donation emails that are at least 30 days old.  We've also added a nightly process
-- to prevent missed emails in the future (see crds_service_clean_donation_emails_nightly)

-- Don't send emails that are older than 30 days
DECLARE @minDate DATE = DATEADD(d, -30, GETDATE())

DECLARE @idsToDelete TABLE (
    Donation_Communications_Id INT NOT NULL,
    Communication_Id INT NOT NULL
);

-- Find donations that have unsent email (Draft status) older than 30 days
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
    c.Communication_Status_ID = 1 AND       -- 1 = Draft
    c.Template = 0 AND
    c.Start_Date < @minDate
;

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

-- Some rows in cr_Donation_Communications reference emails that have already been sent.
-- These should have been deleted from cr_Donation_Communications at the time the emails
-- were sent.
DELETE dc
FROM
    cr_Donation_Communications dc
    INNER JOIN dp_Communications c ON c.Communication_Id = dc.Communication_Id
WHERE
    c.Communication_Status_ID = 4           -- 4 = Sent
;
