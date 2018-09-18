USE [MinistryPlatform]
GO

-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-05-24
-- Description:	Updates Invoice Detail page to
-- show the Invoice ID on the selected record expression
-- =============================================

UPDATE dp_Pages SET Selected_Record_Expression =
'''#'' + CONVERT(VARCHAR(12),Invoice_ID) + '' Invoice'''
WHERE Page_ID = 272 AND Table_Name = 'Invoice_Detail'

---- Rollback
--UPDATE dp_Pages SET Selected_Record_Expression =
--'Invoice_ID_Table.Invoice_Date'
--WHERE Page_ID = 272 AND Table_Name = 'Invoice_Detail'