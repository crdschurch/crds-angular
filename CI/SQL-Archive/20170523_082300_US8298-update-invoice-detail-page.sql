USE [MinistryPlatform]
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-05-23
-- Description:	Adds Item_Note column to default field list on Invoice_Detail, 
-- to support having these tags available for a template or email
-- =============================================

UPDATE dp_Pages SET Default_Field_List =
'Invoice_ID_Table.Invoice_Date
,Invoice_Detail.Invoice_ID AS InvoiceID
,Product_ID_Table.Product_Name
,ISNULL(Product_Option_Price_ID_Table.Option_Title, ''None'') AS Option_Title
,Invoice_Detail.Line_Total
,Recipient_Contact_ID_Table.Display_Name AS [Recipient]
,Invoice_ID_Table_Purchaser_Contact_ID_Table.Display_Name AS [Purchaser]
,(SELECT SUM(Payment_Amount) FROM Payment_Detail PD WHERE Invoice_Detail.Invoice_Detail_ID = PD.Invoice_Detail_ID) AS Amount_Paid
,Event_Participant_ID_Table_Event_ID_Table.Event_Title
,Event_Participant_ID_Table_Event_ID_Table.Event_Start_Date
,Event_Participant_ID_Table_Participation_Status_ID_Table.Participation_Status
,Product_Option_Price_ID_Table.Option_Price
,Item_Quantity
,Item_Note'
WHERE Page_ID = 272 AND Table_Name = 'Invoice_Detail'

---- Rollback
--UPDATE dp_Pages SET Default_Field_List =
--'Invoice_ID_Table.Invoice_Date
--,Invoice_Detail.Invoice_ID AS InvoiceID
--,Product_ID_Table.Product_Name
--,ISNULL(Product_Option_Price_ID_Table.Option_Title, 'None') AS Option_Title
--,Invoice_Detail.Line_Total
--,Recipient_Contact_ID_Table.Display_Name AS [Recipient]
--,Invoice_ID_Table_Purchaser_Contact_ID_Table.Display_Name AS [Purchaser]
--,(SELECT SUM(Payment_Amount) FROM Payment_Detail PD WHERE Invoice_Detail.Invoice_Detail_ID = PD.Invoice_Detail_ID) AS Amount_Paid
--,Event_Participant_ID_Table_Event_ID_Table.Event_Title
--,Event_Participant_ID_Table_Event_ID_Table.Event_Start_Date
--,Event_Participant_ID_Table_Participation_Status_ID_Table.Participation_Status
--,Product_Option_Price_ID_Table.Option_Price
--,Item_Quantity'
--WHERE Page_ID = 272 AND Table_Name = 'Invoice_Detail'