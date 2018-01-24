USE [MinistryPlatform]
GO

DECLARE @Template_ID int = 2034;

IF NOT EXISTS(SELECT 1 FROM [dbo].[dp_Communications] WHERE Communication_ID = @Template_ID) 
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON
	INSERT INTO [dbo].[dp_Communications]
			   (Communication_ID
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
			   (@Template_ID
			   ,1
			   ,N'Password Reset Link - Mobile'
			   ,N'<p>Someone requested that the password be reset for the user with this email address on crossroads.net.</p><div><p>If this was a mistake, just ignore this email and nothing will happen.</p></div><div><p>Use this <a href="[resetlink]">link</a> to reset your password</p></div>'
			   ,1
			   ,'2018-01-10'
			   ,7
			   ,7			   
			   ,1
			   ,1)
	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END
