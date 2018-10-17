
USE [MinistryPlatform]
GO

-- Modify default value on column
--ALTER TABLE [dbo].[Donors] DROP CONSTRAINT [DF_Donors_Statement_Type_ID]
--GO

--ALTER TABLE [dbo].[Donors] ADD  CONSTRAINT [DF_Donors_Statement_Type_ID]  DEFAULT ((1)) FOR [Statement_Type_ID]
--GO

-- Set new defaults in config settings
--UPDATE [dbo].[dp_Configuration_Settings]
--SET Value = 1
--WHERE Key_Name in ('StatementTypeID','DefaultStatementType','DefaultStatementTypeID')
--GO

UPDATE dbo.Donors
SET Statement_Type_ID = 1
WHERE Statement_Type_ID = 2
GO

