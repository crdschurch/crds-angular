/*
exec MPIdentityMaintenance.dbo.Get_Next_Available_ID 'dp_Reports', 'This is a test report used by Nick for mailing postal statements.'
Report_ID = 336
page_ID =299
Descriptions = Generates a list of CRDS Contribution Statement Postal Mail for a user-defined date range.
report path = /Crossroads/CRDS ContributionStatement_Postal_Mail
Report_Name = CRDS Contribution Statements Postal Mail

Select * from dp_reports where Report_ID = 336
Select * from dp_Report_Pages where Report_ID = 336
*/


USE MinistryPlatform
GO

DECLARE @REPORT_ID int = 336
DECLARE @DONORS_PAGE_ID int = 299;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Reports] WHERE [Report_ID] = @REPORT_ID)
BEGIN
    SET IDENTITY_INSERT [dbo].[dp_Reports] ON
    INSERT INTO [dbo].[dp_Reports] (
   	  [Report_ID]
   	 ,[Report_Name]
   	 ,[Description]
   	 ,[Report_Path]
   	 ,[Pass_Selected_Records]
   	 ,[Pass_LinkTo_Records]
   	 ,[On_Reports_Tab]
    ) VALUES (
   	  @REPORT_ID
   	 ,N'CRDS Contribution Statements Postal Mail'
   	 ,N'Generates a list of CRDS Contribution Statement Postal Mail for a user-defined date range.'
   	 ,N'/Crossroads/CRDS ContributionStatement_Postal_mail'
   	 ,0
   	 ,0
   	 ,1
    )
    SET IDENTITY_INSERT [dbo].[dp_Reports] OFF
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Report_Pages] WHERE [Report_ID] = @REPORT_ID  AND [Page_ID] = @DONORS_PAGE_ID)
BEGIN
    INSERT INTO [dbo].[dp_Report_Pages] VALUES (@REPORT_ID, @DONORS_PAGE_ID)
END

