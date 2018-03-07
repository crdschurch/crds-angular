USE [MinistryPlatform]
GO

declare @PageToolAppearsOn int = 297 -- Donation Page
declare @Role int = 2 -- Administrator

/* DEVELOPMENT TOOL */
declare @ToolNameDev varchar(30) = 'Check Batch Processor - Dev'
declare @LaunchPageDev varchar(100) = 'http://localhost:8080/mptools/checkBatchProcessor'
declare @DescriptionDev varchar(50) = 'Development Processor for EZ Scan checks'
declare @ToolIdDev int

IF EXISTS ( SELECT 1 FROM [dbo].[dp_Tools] WHERE [Tool_Name] = @ToolNameDev )
BEGIN
  SELECT @ToolIdDev = [Tool_ID] FROM [dbo].[dp_Tools] WHERE [Tool_Name] = @ToolNameDev
END
ELSE
BEGIN
  INSERT INTO [dbo].[dp_Tools] ([Tool_Name], [Description], [Launch_Page])
  VALUES (@ToolNameDev, @DescriptionDev, @LaunchPageDev)

  SELECT @ToolIdDev = [Tool_ID] FROM [dbo].[dp_Tools] WHERE [Tool_Name] = @ToolNameDev
END

IF NOT EXISTS ( SELECT 1 FROM [dbo].[dp_Tool_Pages] WHERE [Tool_ID] = @ToolIdDev AND [Page_ID] = @PageToolAppearsOn )
BEGIN
  INSERT INTO [dbo].[dp_Tool_Pages] ([Tool_ID], [Page_ID])
  VALUES (@ToolIdDev, @PageToolAppearsOn)
END

-- Give a role permission to access the dev tool
IF NOT EXISTS ( SELECT 1 FROM [dbo].[dp_Role_Tools] WHERE [Role_ID] = @Role AND [Tool_ID] = @ToolIdDev )
BEGIN
  INSERT INTO [dbo].[dp_Role_Tools] ([Tool_ID], [Role_ID], [Domain_ID])
  VALUES (@ToolIdDev, @Role, 1)
END

GO
