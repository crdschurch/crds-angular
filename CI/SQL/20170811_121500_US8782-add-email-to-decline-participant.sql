USE MinistryPlatform

DECLARE @GroupUserDeclinedEmailId INT = 2028;

IF NOT EXISTS(SELECT * FROM [dbo].[dp_Communications] WHERE communication_id =@GroupUserDeclinedEmailId)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON;

	INSERT INTO [dbo].[dp_Communications](
				Communication_ID, 
				Author_User_ID, 
				Subject, 
				Body, 
				Domain_ID, 
				Start_Date, 
				Communication_Status_ID, 
				From_Contact, 
				Reply_to_Contact, 
				Template, 
				Active)
	        VALUES (
				@GroupUserDeclinedEmailId,
				5,
				'The [Group_Name] group isn''t adding people right now',
				'<blockquote style="margin: 0px 0px 0px 40px; border: none; padding: 0px;"><div style="font-family: arial, sans-serif; font-size: 12px;">Hi [Nickname], <br /><br />Thanks for reaching out to try the [Group_Name] group. Unfortunately the group isn''t adding new members at this time. How about trying a different group? You can <span style="text-decoration-line: underline;">search for a new one</span>. And if you don''t see one that quite fits your needs, you can always <span style="text-decoration-line: underline;">start your own group</span>!<br /><br />Thanks again, <br />Crossroads Spiritual Growth Team <br /></div></blockquote>',
				1,
				GETDATE(),
				1,
				7676252,
				7676252,
				1,
				1);

	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF;
END

GO
