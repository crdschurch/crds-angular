USE MinistryPlatform
GO

DECLARE @REPORT_ID INT = 321 
DECLARE @DONORS_PAGE_ID INT = 299

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
   	 ,N'CRDS Contribution Statement Selected'
   	 ,N'CRDS Contribution Statement for selected donors'
   	 ,N'/MPReports/Crossroads/CRDS ContributionStatement'
   	 ,1
   	 ,0
   	 ,1
    )
    SET IDENTITY_INSERT [dbo].[dp_Reports] OFF
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Report_Pages] WHERE [Report_ID] = @REPORT_ID AND [Page_ID] = @DONORS_PAGE_ID)
BEGIN
    INSERT INTO [dbo].[dp_Report_Pages] VALUES (@REPORT_ID, @DONORS_PAGE_ID)
END

--Grant the same security that CRDS COntribution STatements already has (2,4,106, 107)
IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_reports] WHERE [Report_ID] = @REPORT_ID AND Role_ID = 2)
BEGIN
    INSERT INTO [dbo].[dp_Role_reports] (Report_ID, Role_ID, Domain_ID)  VALUES (@REPORT_ID, 2 , 1)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_reports] WHERE [Report_ID] = @REPORT_ID AND Role_ID = 4)
BEGIN
    INSERT INTO [dbo].[dp_Role_reports] (Report_ID, Role_ID, Domain_ID) VALUES (@REPORT_ID, 4, 1)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_reports]  WHERE [Report_ID] = @REPORT_ID AND Role_ID = 106)
BEGIN
    INSERT INTO [dbo].[dp_Role_reports] (Report_ID, Role_ID, Domain_ID) VALUES (@REPORT_ID, 106, 1)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_reports] WHERE [Report_ID] = @REPORT_ID AND Role_ID = 107)
BEGIN
    INSERT INTO [dbo].[dp_Role_reports] (Report_ID, Role_ID, Domain_ID) VALUES (@REPORT_ID, 107, 1)
END

