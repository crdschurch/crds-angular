USE [MinistryPlatform]
GO

DECLARE @ToolId INT

INSERT INTO [dbo].[dp_Tools]
           ([Tool_Name]
           ,[Description]
           ,[Launch_Page])
     VALUES
           ('Trip Private Invites - DEV'
           ,'Development Trip Private Invite Tool'
           ,'http://localhost:3000/mptools/tripPrivateInvite')

SET @ToolId = SCOPE_IDENTITY()

INSERT INTO [dbo].[dp_Tool_Pages]
           ([Tool_ID]
           ,[Page_ID])
     VALUES
           (@ToolId
           ,514)

INSERT INTO [dbo].[dp_Role_Tools]
           (Role_ID, Tool_ID, Domain_ID)
     VALUES
           (2, @ToolId, 1)
