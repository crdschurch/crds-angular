USE [MinistryPlatform]
GO

DECLARE @ReportId INTEGER = 1011;
DECLARE @GroupsPageId INTEGER = 322;

DECLARE @RoleIdSysAdmin INTEGER = 85;
DECLARE @RoleIdAllStaff INTEGER = 100;
DECLARE @RoleIdGroupManager INTEGER = 94;

-- Report
SET IDENTITY_INSERT dp_Reports ON;

IF(NOT EXISTS(SELECT * FROM dp_Reports WHERE Report_ID = @ReportId))
BEGIN
	INSERT INTO dp_Reports(Report_ID, Report_Name, Description, Report_Path, Pass_Selected_Records, Pass_LinkTo_Records, On_Reports_Tab, Pass_Database_Connection)
	VALUES(@ReportId, 'Group Leader Changes', 'Group Leader Changes','/Crossroads/CRDSGroupLeaderChanges',0,0,1,0)
END
SET IDENTITY_INSERT dp_Reports OFF;

--Report Pages
IF(NOT EXISTS(SELECT * FROM dp_Report_Pages WHERE Report_ID = @ReportId AND Page_ID = @GroupsPageId))
BEGIN
	INSERT INTO dp_Report_Pages(Report_ID, Page_ID) VALUES(@ReportId, @GroupsPageId)
END

--Security
IF(NOT EXISTS(SELECT * FROM dp_Role_Reports WHERE Report_ID = @ReportId AND Role_ID = @RoleIdSysAdmin))
BEGIN
	INSERT INTO dp_Role_Reports(Role_ID,Report_ID,Domain_ID) VALUES(@RoleIdSysAdmin, @ReportId, 1);
END

IF(NOT EXISTS(SELECT * FROM dp_Role_Reports WHERE Report_ID = @ReportId AND Role_ID = @RoleIdAllStaff))
BEGIN
	INSERT INTO dp_Role_Reports(Role_ID,Report_ID,Domain_ID) VALUES(@RoleIdAllStaff, @ReportId, 1);
END

IF(NOT EXISTS(SELECT * FROM dp_Role_Reports WHERE Report_ID = @ReportId AND Role_ID = @RoleIdGroupManager))
BEGIN
	INSERT INTO dp_Role_Reports(Role_ID,Report_ID,Domain_ID) VALUES(@RoleIdGroupManager, @ReportId, 1);
END

GO


