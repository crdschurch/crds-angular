USE [MinistryPlatform]
GO

INSERT INTO [dbo].[dp_Tools]
           ([Tool_Name]
           ,[Description]
           ,[Launch_Page]
           ,[Launch_with_Credentials])
     VALUES
           ('Request Childcare Tool - Dev'
           ,'Request childcare using this tool'
           ,'http://localhost:3000/CRDStools/requestchildcare'
           ,1)
GO

INSERT INTO [dbo].[dp_Tool_Pages]
           ([Tool_ID]
           ,[Page_ID])
     VALUES
           ((SELECT Tool_ID FROM dp_Tools WHERE Tool_Name = 'Request Childcare Tool - Dev')
           ,(SELECT Page_ID FROM dp_Pages WHERE Display_Name = 'My Childcare Requests'))
GO


