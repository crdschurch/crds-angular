USE [MinistryPlatform]
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Page_Section_Pages] WHERE Page_ID = 38)
BEGIN
	INSERT INTO [dbo].[dp_Page_Section_Pages]
           ([Page_ID]
           ,[Page_Section_ID])
     VALUES
           (38
           ,5)
END
GO