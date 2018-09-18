USE [MinistryPlatform]
GO

DECLARE @Template_ID INT = 2020;
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
		, N'Group Leader Application - Interview Needed'
		, N'<div>A community member has applied to be a group leader, but doesn''t know anyone on staff to provide them a reference. Please reach out to schedule an interview for them.</div>

<div><br /></div>

<div>First Name: [First_Name]</div>
<div>Last Name: [Last_Name]</div>
<div>Email Address: [Email_Address]</div>'
		, 1
		, '2017-05-24'
		, @Groups_Contact_ID
		, @Groups_Contact_ID
		, 1
		, 1
	)

	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END