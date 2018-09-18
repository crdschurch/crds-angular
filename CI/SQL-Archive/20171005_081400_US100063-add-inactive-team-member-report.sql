USE MinistryPlatform
GO

GO

DECLARE @report_id INT = 326; -- from identity maintainance
DECLARE @page_id INT;

SELECT @page_id = p.Page_ID FROM
	[dbo].[dp_Pages] p WHERE p.Display_Name = N'My Serve Group Participants'

IF NOT EXISTS(SELECT * FROM [dbo].[dp_reports] WHERE Report_Path = '/MPReports/Crossroads/CRDS Team Leader 90 Day Inactive')
BEGIN
	SET IDENTITY_INSERT dp_Reports ON
	INSERT INTO [dbo].[dp_reports]
			   ([Report_ID],
			    [Report_Name],
				[Description],
				[Report_Path],
				[Pass_Selected_Records],
				[Pass_LinkTo_Records],
				[On_Reports_Tab]
			   )
		 VALUES
			   (@report_id
			   ,N'90 Day Inactive Serve Team Members'
			   ,N'90 Day Inactive Serve Team Members'
			   ,N'/MPReports/CRDS Team Leader 90 Day Inactive'
			   ,1
			   ,0          
			   ,1)
	SET IDENTITY_INSERT dp_Reports OFF
END


IF NOT EXISTS(SELECT * FROM [dbo].[dp_Report_Pages] WHERE Report_ID = @report_id AND Page_ID = @page_id )
BEGIN
	INSERT INTO [dbo].[dp_Report_Pages]
		(
			[Report_ID],
			[Page_ID]
		)
		VALUES
		(
			@report_id,
			@page_id
		)
END
GO


IF NOT EXISTS ( SELECT * FROM dp_Role_Reports 
                WHERE  Role_ID = 100
                   AND Report_ID = 326) 
 BEGIN
	INSERT INTO [dbo].[dp_Role_Reports]([Role_ID],[Report_ID],[Domain_ID])
	VALUES (100,326,1 )
 END

 IF NOT EXISTS ( SELECT * FROM dp_Role_Reports 
                WHERE  Role_ID = 107
                   AND Report_ID = 326) 
 BEGIN
	INSERT INTO [dbo].[dp_Role_Reports]([Role_ID],[Report_ID],[Domain_ID])
	VALUES (107,326,1 )
 END