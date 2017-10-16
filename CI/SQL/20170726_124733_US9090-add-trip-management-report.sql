USE [MinistryPlatform]
GO

DECLARE @ReportID int = 324;
DECLARE @PageID int = 308;
DECLARE @TripReportsRole int = 108;

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
		   ,'Trip Management Report'
           ,'Trip Management Report'
           ,'/MPReports/Crossroads/CRDS Trip Management'
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

IF NOT EXISTS(SELECT 1 FROM dp_Role_Reports WHERE Role_ID = @TripReportsRole AND Report_ID = @ReportID)
BEGIN
INSERT INTO [dbo].[dp_Role_Reports]
           ([Role_ID]
           ,[Report_ID]
           ,[Domain_ID])
     VALUES
           (@TripReportsRole
           ,@ReportID
           ,1)
END
