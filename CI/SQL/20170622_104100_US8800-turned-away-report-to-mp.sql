USE MinistryPlatform
GO

-- Author: John Cleaver (john.cleaver@ingagepartners.com)
-- Description: Create entry for report and matching report page

DECLARE @report_id INT;
DECLARE @page_id INT;

SELECT @page_id = p.Page_ID FROM
	[dbo].[dp_Pages] p WHERE p.Display_Name = N'Events'

IF NOT EXISTS(SELECT * FROM [dbo].[dp_reports] WHERE Report_Path = '/MPReports/Crossroads/CRDS Checkin Turned Away Summary')
BEGIN

	SET IDENTITY_INSERT [dbo].[dp_Reports] ON

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
			   (323
			   ,N'CRDS Checkin Turned Away Summary'
			   ,N'CRDS Checkin Turned Away Summary'
			   ,N'/MPReports/Crossroads/CRDS Checkin Turned Away Summary'
			   ,0
			   ,0          
			   ,1)
			   
	SET IDENTITY_INSERT [dbo].[dp_Reports] OFF
END

SELECT @report_id = r.Report_ID FROM
	[dp_reports] r WHERE r.Report_Path = N'/MPReports/Crossroads/CRDS Checkin Turned Away Summary'

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
