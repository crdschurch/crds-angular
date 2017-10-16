USE [MinistryPlatform]
GO

DECLARE @TOOL_ID int = 425;
DECLARE @EVENT_PAGE_ID int = 308;
DECLARE @TRIP_TOOLS_ROLE_ID int = 113;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Tools] WHERE Tool_ID = @TOOL_ID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Tools] ON

	INSERT INTO [dbo].[dp_Tools]
           ([Tool_ID]
		   ,[Tool_Name]
           ,[Description]
           ,[Launch_Page]
           ,[Launch_with_Credentials])
     VALUES
           (@TOOL_ID
		   ,N'Trip Setup'
           ,N'Setup a new GO Trip'
           ,N'/crds-mp-tools/tripsetup'
           ,1)

	SET IDENTITY_INSERT [dbo].[dp_Tools] OFF
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Tool_Pages] WHERE [Tool_ID] = @TOOL_ID AND [Page_ID] = @EVENT_PAGE_ID)
BEGIN
	INSERT INTO [dbo].[dp_Tool_Pages] (
		 Tool_ID
		,Page_ID
	) VALUES (
		 @TOOL_ID
		,@EVENT_PAGE_ID
	)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_Tools] WHERE [Role_ID] = @TRIP_TOOLS_ROLE_ID AND [Tool_ID] = @TOOL_ID)
BEGIN
	INSERT INTO [dbo].[dp_Role_Tools] (
		 [Role_ID]
		,[Tool_ID]
	) VALUES (
	    @TRIP_TOOLS_ROLE_ID
	   ,@TOOL_ID
	)
END



