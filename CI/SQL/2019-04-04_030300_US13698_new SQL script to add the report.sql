USE MinistryPlatform
GO

DECLARE @REPORT_ID int = 331
DECLARE @DONORS_PAGE_ID int = 322;

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
   	 ,N'Weekend Service - FI Report'
   	 ,N'Generate a list of volunteers to serve with the FI Teams.'
   	 ,N'/Crossroads/Weekend Service Crossroads_FI'
   	 ,0
   	 ,0
   	 ,1
    )
    SET IDENTITY_INSERT [dbo].[dp_Reports] OFF
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Report_Pages] WHERE [Report_ID] = @REPORT_ID AND [Page_ID] = @DONORS_PAGE_ID)
BEGIN
    INSERT INTO [dbo].[dp_Report_Pages] VALUES (@REPORT_ID, @DONORS_PAGE_ID)
END

