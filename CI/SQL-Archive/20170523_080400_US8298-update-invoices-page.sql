USE [MinistryPlatform]
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-05-23
-- Description:	Adds Notes column to default field list on Invoices, 
-- to support having these tags available for a template or email
-- =============================================

UPDATE dp_Pages SET Default_Field_List =
'Invoice_Date
,Purchaser_Contact_ID_Table.Display_Name
,Purchaser_Contact_ID_Table.Nickname
,Purchaser_Contact_ID_Table.First_Name
,Invoice_Total
,Invoice_Status_ID_Table.Invoice_Status
,Invoices.Invoice_ID AS InvoiceID
,YEAR(Invoices.Invoice_Date) AS Invoice_Year
,DATENAME(M,Invoices.Invoice_Date) AS Invoice_Month
,MONTH(Invoices.Invoice_Date) AS Invoice_Month_Number
,Notes'
WHERE Page_ID = 330 AND Table_Name = 'Invoices'

---- Rollback
--UPDATE dp_Pages SET Default_Field_List =
--'Invoice_Date
--,Purchaser_Contact_ID_Table.Display_Name
--,Purchaser_Contact_ID_Table.Nickname
--,Purchaser_Contact_ID_Table.First_Name
--,Invoice_Total
--,Invoice_Status_ID_Table.Invoice_Status
--,Invoices.Invoice_ID AS InvoiceID
--,YEAR(Invoices.Invoice_Date) AS Invoice_Year
--,DATENAME(M,Invoices.Invoice_Date) AS Invoice_Month
--,MONTH(Invoices.Invoice_Date) AS Invoice_Month_Number'
--WHERE Page_ID = 330 AND Table_Name = 'Invoices'