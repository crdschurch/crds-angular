USE [MinistryPlatform]
GO

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Sort_Order' AND Object_ID = Object_ID(N'Attribute_Categories'))
BEGIN

BEGIN TRANSACTION
ALTER TABLE dbo.Attribute_Categories ADD
	Sort_Order int NULL

COMMIT TRANSACTION
END

BEGIN

BEGIN TRANSACTION

UPDATE dbo.Attribute_Categories	
SET Sort_Order = 0
WHERE Attribute_Category_ID = 51

UPDATE dbo.Attribute_Categories	
SET Sort_Order = 1
WHERE Attribute_Category_ID = 20

UPDATE dbo.Attribute_Categories	
SET Sort_Order = 2
WHERE Attribute_Category_ID = 18

UPDATE dbo.Attribute_Categories	
SET Sort_Order = 3
WHERE Attribute_Category_ID = 19

UPDATE dbo.Attribute_Categories	
SET Sort_Order = 4
WHERE Attribute_Category_ID = 17

UPDATE dbo.Attribute_Categories	
SET Sort_Order = 5
WHERE Attribute_Category_ID = 21

COMMIT TRANSACTION
END
