USE MinistryPlatform;
GO

DECLARE @TEMPLATE_ID int = 84;
DECLARE @SUBJECT nvarchar(500) = N'Childcare Requested on [Childcare_Date]!'
DECLARE @BODY nvarchar(1000) = N'Hey [Nickname],<br /><br />
We just wanted to remind you that you requested childcare for [Childcare_Day]. If your plans have changed, please let us know by updating your information at <a href="[Base_URL]/childcare">[Base_URL]/childcare</a>
<br /><br />
Crossroads';

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Communications] WHERE [Subject] = @SUBJECT AND [Template] = 1)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON
	INSERT INTO [dbo].[dp_Communications] (
		 [Communication_ID]
		,[Author_User_ID]
		,[Subject]
		,[Body]
		,[Domain_ID]
		,[Start_Date]
		,[From_Contact]
		,[Reply_to_Contact]
		,[Template]
		,[Active]		
	) VALUES (
		 @TEMPLATE_ID
		,5
		,@SUBJECT
		,@BODY
		,1
		,GETDATE()
		,7
		,7
		,1
		,1
	)
	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END
