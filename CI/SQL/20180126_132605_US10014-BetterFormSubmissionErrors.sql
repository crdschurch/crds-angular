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
		,N'[Form_Name] Form Submission Failed'
		,N'A form submission for [Form_Name] has failed. Details are below: <div><br /></div><div>User Name: [User_Name]</div><div><br /></div><div>Submission Date: [Submission_Date]</div><div><br /></div><div>Form Data:[Submission_Data]</div><div><br /></div>'
		,1
		,@From_Contact_ID
		,@From_Contact_ID
		,1
	);
	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF;
END

DECLARE @Template_ID int = 2036;
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
		,N'Invoice Creation for [Form_Name] form Failed'
		,N'A part of the invoice creation process failed when [Form_Name] form was submitted. Details are below: <div><br /></div><div>Contact Id: [Contact_ID]</div><div><br /></div><div>Submission Date: [Submission_Date]</div><div><br /></div><div>Form Data:[Submission_Data]</div><div><br /></div>'
		,1
		,@From_Contact_ID
		,@From_Contact_ID
		,1
	);
	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF;
END

DECLARE @AppCode NVARCHAR(32) = 'COMMON'

IF NOT EXISTS(SELECT * FROM dp_Configuration_Settings WHERE Application_Code = @AppCode AND Key_Name = 'mpAdminContactId')
BEGIN
 INSERT INTO dp_Configuration_Settings(Application_Code,Key_Name,Value,Description,Domain_ID)
	VALUES(@AppCode,'mpAdminContactId','1','Contact ID for Ministry Platform Administrator',1)
END

IF NOT EXISTS(SELECT * FROM dp_Configuration_Settings WHERE Application_Code = @AppCode AND Key_Name = 'helpDeskContactId')
BEGIN
 INSERT INTO dp_Configuration_Settings(Application_Code,Key_Name,Value,Description,Domain_ID)
	VALUES(@AppCode,'helpDeskContactId','4464251','Contact ID for the CRDS Help Desk',1)
END

DECLARE @AppCode NVARCHAR(32) = 'FRED'

IF NOT EXISTS(SELECT * FROM dp_Configuration_Settings WHERE Application_Code = @AppCode AND Key_Name = 'invoiceFailureEmailTemplateId')
BEGIN
 INSERT INTO dp_Configuration_Settings(Application_Code,Key_Name,Value,Description,Domain_ID)
	VALUES(@AppCode,'invoiceFailureEmailTemplateId','2036','Email templtes ID used by FRED when creating an invoice fails',1)
END

GO