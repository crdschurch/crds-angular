USE [MinistryPlatform]
GO

declare @PageToolAppearsOn int = 299 -- Donors Page
declare @Role int = 7 -- Stewardship Donation Processor

/* DEVELOPMENT TOOL */
declare @toolidsdev table (id int);
declare @ToolNameDev varchar(30) = 'Donor Giving History - Dev'
declare @LaunchPageDev varchar(100) = 'http://localhost:8080/mptools/adminGivingHistoryTool'
declare @DescriptionDev varchar(50) = 'View the giving history for a Donor.'
declare @ToolIdDev int


IF EXISTS (
  SELECT 1
  FROM [dbo].[dp_Tools]
  WHERE [dbo].[dp_Tools].[Tool_Name] = @ToolNameDev
  )
BEGIN
  UPDATE [dbo].[dp_Tools]
  SET  [Description] = @DescriptionDev
    ,[Launch_Page] = @LaunchPageDev
  OUTPUT INSERTED.Tool_ID INTO @toolidsdev
  WHERE [Tool_Name] = @ToolNameDev

  -- Store the id of the updated record in a variable
  SELECT TOP 1 @ToolIdDev = id from @toolidsdev
END
ELSE
BEGIN
  INSERT INTO [dbo].[dp_Tools]
           ([Tool_Name]
           ,[Description]
           ,[Launch_Page])
     VALUES
           (@ToolNameDev
           ,@DescriptionDev
           ,@LaunchPageDev)

  -- Store the id of the inserted record in a variable
  SET @ToolIdDev = SCOPE_IDENTITY()
END

IF NOT EXISTS (
  SELECT 1
  FROM [dbo].[dp_Tool_Pages]
  WHERE [dbo].[dp_Tool_Pages].[Tool_ID] = @ToolIdDev
  AND [dbo].[dp_Tool_Pages].[Page_ID] = @PageToolAppearsOn
)
BEGIN
  INSERT INTO [dbo].[dp_Tool_Pages]
  ([Tool_ID],
   [Page_ID])
   VALUES (@ToolIdDev, @PageToolAppearsOn)
END

-- Give a role permission to access the dev tool
IF NOT EXISTS
  ( SELECT 1
    FROM [dbo].[dp_Role_Tools]
    WHERE [dbo].[dp_Role_Tools].[Role_ID] = @Role AND [dbo].[dp_Role_Tools].[Tool_ID] = @ToolIdDev )
BEGIN
  INSERT INTO [dbo].[dp_Role_Tools]
  ([Tool_ID], [Role_ID], [Domain_ID])
  VALUES (@ToolIdDev, @Role, 1)
END

GO
