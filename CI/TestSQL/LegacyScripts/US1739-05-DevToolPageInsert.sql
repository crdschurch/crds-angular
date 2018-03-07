USE [MinistryPlatform]
GO

INSERT INTO [dbo].[dp_Tool_Pages]
           ([Tool_ID]
           ,[Page_ID])
     VALUES
           (43
           ,424)

INSERT INTO [dbo].[dp_Role_Tools]
           (Role_ID, Tool_ID, Domain_ID)
     VALUES
           (2, 43, 1)
GO