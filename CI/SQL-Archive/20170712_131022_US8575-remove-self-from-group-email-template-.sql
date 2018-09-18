USE [MinistryPlatform]
GO

DECLARE @Template_ID INT = 2023;
DECLARE @Groups_Contact_ID INT = 7675411;

IF NOT EXISTS(SELECT 1 FROM [dbo].[dp_Communications] WHERE Communication_ID = @Template_ID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON
	
	INSERT INTO [dbo].[dp_Communications]
	(
		Communication_ID
		, [Author_User_ID]
		, [Subject]
		, [Body]
		, [Domain_ID]
		, [Start_Date]
		, [From_Contact]
		, [Reply_To_Contact]
		, [Template]
		, [Active]
	)
	VALUES
	(
		@Template_ID
		, 1
		, N'Group Member Removed Self From Your Group, [Group_Name]'
		, N'<div>A community member has removed themself from your group.</div><div><br /></div> <div>Name: [Group_Participant_Name]</div><div>Group Name: [Group_Name]</div>'
		, 1
		, GetDate()
		, @Groups_Contact_ID
		, @Groups_Contact_ID
		, 1
		, 1
	)

	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END