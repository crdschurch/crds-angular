USE [MinistryPlatform]
GO

DECLARE @ReportID INT = 329;
DECLARE @PageID INT = 517;
DECLARE @ReportRoles TABLE (
    Role_ID INT NOT NULL
);

INSERT INTO @ReportRoles
    (Role_ID)
VALUES
    (2),        -- Administrators
    (7),        -- Stewardship Donation Processor
    (105),      -- Finance Donation Mgr - CRDS
    (106),      -- Finance Management - CRDS
    (107)       -- System Administrator - CRDS
;    

IF NOT EXISTS(SELECT 1 FROM dp_Reports WHERE Report_ID = @ReportID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Reports] ON
	INSERT INTO [dbo].[dp_Reports]
           ([Report_ID]
		   ,[Report_Name]
           ,[Description]
           ,[Report_Path]
           ,[Pass_Selected_Records]
           ,[Pass_LinkTo_Records]
           ,[On_Reports_Tab]
           ,[Pass_Database_Connection])
     VALUES
           (@ReportID
		   ,'CRDS Declined Recurring Gifts'
           ,'Lists the recurring gifts where the most-recent transaction was declined.'
           ,'/Crossroads/CRDS Declined Recurring Gifts'
           ,0
           ,0
           ,1
           ,0)
	SET IDENTITY_INSERT [dbo].[dp_Reports] OFF
END

IF NOT EXISTS(SELECT 1 FROM dp_Report_Pages WHERE Report_ID = @ReportID AND Page_ID = @PageID)
BEGIN
INSERT INTO [dbo].[dp_Report_Pages]
           ([Report_ID]
           ,[Page_ID])
     VALUES
           (@ReportID
           ,@PageID)
END

INSERT INTO dp_Role_Reports
    (Role_ID, Report_ID, Domain_ID)
SELECT
    newRoles.Role_ID,
    @ReportID,
    1
FROM
    @ReportRoles newRoles
    LEFT JOIN dp_Role_Reports rr ON rr.Role_ID = newRoles.Role_ID AND rr.Report_ID = @ReportID AND rr.Domain_ID = 1
WHERE
    rr.Role_Report_ID IS NULL
;
