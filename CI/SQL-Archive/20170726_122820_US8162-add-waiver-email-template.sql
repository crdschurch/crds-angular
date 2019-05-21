USE [MinistryPlatform]
GO

DECLARE @TemplateID int = 2024;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Communications] WHERE [Communication_ID] = @TemplateID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON
	INSERT INTO [dbo].[dp_Communications]
			   ([Communication_ID]
			   ,[Author_User_ID]
			   ,[Subject]
			   ,[Body]
			   ,[Domain_ID]
			   ,[Start_Date]
			   ,[From_Contact]
			   ,[Reply_to_Contact]
			   ,[Template]
			   ,[Active])
		 VALUES
			   (@TemplateID
			   ,3009216 -- Crossroads Church
			   ,N'Action Required: Follow the instructions to finish signing your waiver.'
			   ,N'To finish the process of signing your waiver for [Event_Name] click <a href="[Confirmation_Url]">this confirmation</a>. <div><br /></div><div>If you received this email by mistake, simply delete it.<br /><br /></div><div>Thanks,</div><div>Crossroads Reachout</div>'
			   ,1
			   ,GETDATE()
			   ,1519180
			   ,1519180
			   ,1
			   ,1)
	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END


