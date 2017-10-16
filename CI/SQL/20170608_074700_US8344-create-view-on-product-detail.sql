USE [MinistryPlatform]
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-06-08
-- Description:	Updates Deposits and Products Page View
-- =============================================

DECLARE @PageViewID int = 1122

IF EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PageViewID)
BEGIN
	UPDATE [dbo].[dp_Page_Views]
	SET [Field_List]='Invoice_Detail_ID_Table_Product_ID_Table.[Product_Name] AS [Product Name]
		, Payment_ID_Table_Batch_ID_Table.[Batch_ID] AS [Batch ID]
		, Payment_ID_Table_Batch_ID_Table_Deposit_ID_Table.[Deposit_ID] AS [Deposit ID]
		, Payment_ID_Table.[Invoice_Number] AS [Invoice Number]
		, Payment_Detail.[Payment_Amount] AS [Payment Amount]
		, Payment_ID_Table.[Payment_Date] AS [Payment Date]
		, Invoice_Detail_ID_Table_Invoice_ID_Table_Purchaser_Contact_ID_Table.[Display_Name] AS [Purchaser]
		, Invoice_Detail_ID_Table_Recipient_Contact_ID_Table.[Display_Name] AS [Recipient]
		, Invoice_Detail_ID_Table_Event_Participant_ID_Table_Event_ID_Table.[Event_Title] AS [Event Name]
		, Invoice_Detail_ID_Table_Event_Participant_ID_Table_Event_ID_Table.[Event_Start_Date] AS [Event Start Date]'
	WHERE Page_View_ID = @PageViewID AND View_Title='Deposits and Products'
END
