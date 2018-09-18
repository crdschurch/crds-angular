USE MinistryPlatform
GO

DECLARE @REPORT_ID INT = 320
DECLARE @BUILDINGS_PAGE_ID INT = 283
DECLARE @EVENTS_PAGE_ID INT = 308
DECLARE @ROOM_RESERVATIONS_PAGE_ID INT = 384
DECLARE @ROOMS_PAGE_ID INT = 386

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
   	 ,N'CRDS Rooms Available Research'
   	 ,N'Generate a list of rooms which are available for reservation'
   	 ,N'/MPReports/Crossroads/CRDS Rooms Available'
   	 ,0
   	 ,0
   	 ,1
    )
    SET IDENTITY_INSERT [dbo].[dp_Reports] OFF
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Report_Pages] WHERE [Report_ID] = @REPORT_ID AND [Page_ID] = @BUILDINGS_PAGE_ID)
BEGIN
    INSERT INTO [dbo].[dp_Report_Pages] VALUES (@REPORT_ID, @BUILDINGS_PAGE_ID)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Report_Pages] WHERE [Report_ID] = @REPORT_ID AND [Page_ID] = @EVENTS_PAGE_ID)
BEGIN
    INSERT INTO [dbo].[dp_Report_Pages] VALUES (@REPORT_ID, @EVENTS_PAGE_ID)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Report_Pages] WHERE [Report_ID] = @REPORT_ID AND [Page_ID] = @ROOM_RESERVATIONS_PAGE_ID)
BEGIN
    INSERT INTO [dbo].[dp_Report_Pages] VALUES (@REPORT_ID, @ROOM_RESERVATIONS_PAGE_ID)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Report_Pages] WHERE [Report_ID] = @REPORT_ID AND [Page_ID] = @ROOMS_PAGE_ID)
BEGIN
    INSERT INTO [dbo].[dp_Report_Pages] VALUES (@REPORT_ID, @ROOMS_PAGE_ID)
END