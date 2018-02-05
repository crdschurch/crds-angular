USE MinistryPlatform
GO

DECLARE @Template_ID int = 2035;
DECLARE @From_Contact_ID int = 1;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Communications] WHERE [Communication_ID] = @Template_ID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON;
	INSERT INTO [dbo].[dp_Communications] (
		 [Communication_ID]
		,[Author_User_ID]
		,[Subject]
		,[Body]
		,[Domain_ID]
		,[From_Contact]
		,[Reply_to_Contact]
		,[Template]
	) VALUES (
		 @Template_ID
		,5
		,N'[Event] for [Form_Name] Failed'
		,N'[Event] for [Form_Name] has failed. Details are below: <div><br /></div><div>User Name: [User_Name]</div><div><br /></div><div>Submission Date: [Submission_Date]</div><div><br /></div><div>Form Data:[Submission_Data]</div><div><br /></div>'
		,1
		,@From_Contact_ID
		,@From_Contact_ID
		,1
	);
	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF;
END