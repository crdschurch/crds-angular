USE MinistryPlatform
GO

DECLARE @report_id int;
DECLARE @page_id int;

SELECT @page_id = p.Page_ID FROM
	[dbo].[dp_Pages] p WHERE p.Display_Name = N'Deposits'


IF NOT EXISTS(SELECT * FROM [dbo].[dp_reports] WHERE Report_Path = '/MPReports/Crossroads/CRDS Selected Deposit Export Preview')
BEGIN
    SET IDENTITY_INSERT dbo.dp_Reports ON
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
			   (305
			   ,N'Selected Deposit - Export Preview'
			   ,N'Selected Deposit - Export Preview (Donations and Payments)'
			   ,N'/MPReports/Crossroads/CRDS Selected Deposit Export Preview'
			   ,1
			   ,1          
			   ,0)
END

SELECT @report_id = r.Report_ID FROM
	[dp_reports] r WHERE r.Report_Path = N'/MPReports/Crossroads/CRDS Selected Deposit Export Preview'

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
