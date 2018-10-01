use MinistryPlatform
Go

DECLARE @Template_ID int = 2022;
DECLARE @From_Contact_ID int = 7680233;

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
		,N'[Product_Name] Payment Received'
		,N'Thanks for submitting your payment of $[Payment_Total].<div><br /></div><div>Please note that these payments are <b>NOT</b> tax deductible.</div><div><br /></div><div>If you have any questions, email [Primary_Contact_Email].</div><div><br /></div><div>Thanks,</div><div>[Primary_Contact_Display_Name]</div>'
		,1
		,@From_Contact_ID
		,@From_Contact_ID
		,1
	);
	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF;
END
