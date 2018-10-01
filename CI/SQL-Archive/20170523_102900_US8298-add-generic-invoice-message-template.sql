USE [MinistryPlatform]
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-05-23
-- Description:	Adds template for invoice detail
-- (Note: ID created via Identity Maintenance proc)
-- =============================================

DECLARE @TemplateID int = 2019
DECLARE @Body VARCHAR(max) = '<div>Hey there!</div>
<div></br></div>
<div>You have a new invoice for [Product_Name] in the amount of [Line_Total].</div>
<div></br></div>
<div>In order to make a payment, you can do so at any time by going to https://www.crossroads.net/invoices/[InvoiceID].</div>
<div>Please note that these payments are <strong>NOT tax deductible.</strong> If you have any questions, please reach out to your staff contact.</div>
<div></br></div>
<div>Thanks!</div>'
DECLARE @Subject VARCHAR(max) = 'You Have a New Invoice from Crossroads'

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Communications] WHERE Communication_ID = @TemplateID)
BEGIN
	SET IDENTITY_INSERT [dbo].[dp_Communications] ON
	INSERT INTO [dp_Communications]
	(
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
	)
	VALUES
	(
		 @TemplateID
		,5
	    ,@Subject
		,@Body
		,1
		,GetDate()
		,7680233 -- accounts@crossroads.net
		,7680233 -- accounts@crossroads.net
		,1
		,1
	)

	SET IDENTITY_INSERT [dbo].[dp_Communications] OFF
END
