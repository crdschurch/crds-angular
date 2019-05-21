USE [MinistryPlatform]
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-05-24
-- Description:	Adds Page View to All Payment Detail
-- =============================================

DECLARE @PageViewID int = 1122

IF NOT EXISTS(SELECT * FROM dp_Page_Views WHERE Page_View_ID = @PageViewID)
BEGIN
	SET IDENTITY_INSERT dp_Page_Views ON
	INSERT INTO [dbo].[dp_Page_Views]
           ([Page_View_ID]
		   ,[View_Title]
           ,[Page_ID]
           ,[Description]
           ,[Field_List]
           ,[View_Clause])
     VALUES
           (@PageViewID
		   ,'Deposits and Products'
           ,273
           ,'Shows Invoices, Batches, Deposits, and Products'
           ,'Invoice_Detail_ID_Table_Invoice_ID_Table.[Invoice_ID] AS [Invoice ID]
			, Payment_ID_Table_Batch_ID_Table.[Batch_ID] AS [Batch ID]
			, Payment_ID_Table_Batch_ID_Table_Deposit_ID_Table.[Deposit_ID] AS [Deposit ID]
			, Invoice_Detail_ID_Table_Product_ID_Table.[Product_Name] AS [Product Name]'
           ,'1=1')
	SET IDENTITY_INSERT dp_Page_Views OFF
END
