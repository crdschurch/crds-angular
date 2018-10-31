USE [MinistryPlatform]
GO

DECLARE @ReportID INT = 330;
DECLARE @PageID INT = 363;
DECLARE @ReportRoles TABLE (
    Role_ID INT NOT NULL
);

INSERT INTO @ReportRoles
    (Role_ID)
VALUES
    (2),        -- Administrators
    (4),        -- Basic Reports
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
		   ,'CRDS Selected Pledge Mail Merge'
           ,'This report includes data fields necessary for a mail merge related to the selected pledge records including the pledge total, given, and balance.'
           ,'/Crossroads/CRDS Selected Pledge Mail Merge'
           ,1
           ,0
           ,0
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
