USE [MinistryPlatform]
GO

declare @PageToolAppearsOn int = 294 -- Deposit Page
declare @Role int = 2 -- Administrator

/* DEVELOPMENT TOOL */
declare @ToolNameDev varchar(30) = 'GP Deposit Export - Dev'
declare @LaunchPageDev varchar(100) = 'http://localhost:8080/mptools/gpExport'
declare @DescriptionDev varchar(50) = 'Dev to Generate the GP Export tab delimited file.'
declare @ToolIdDev int = 368

SET IDENTITY_INSERT [dbo].[dp_Tools] ON
IF NOT EXISTS ( SELECT 1 FROM [dbo].[dp_Tools] WHERE [Tool_Name] = @ToolNameDev )
BEGIN
  INSERT INTO [dbo].[dp_Tools] ([Tool_ID], [Tool_Name], [Description], [Launch_Page])
  VALUES (@ToolIdDev, @ToolNameDev, @DescriptionDev, @LaunchPageDev)
END
SET IDENTITY_INSERT [dbo].[dp_Tools] OFF

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
