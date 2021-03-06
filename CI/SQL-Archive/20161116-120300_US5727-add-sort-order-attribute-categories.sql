USE MinistryPlatform
GO

IF NOT EXISTS(SELECT * FROM sys.columns 
           WHERE Name = N'Sort_Order' AND Object_ID = Object_ID(N'dbo.Attribute_Categories'))
BEGIN
    ALTER TABLE dbo.Attribute_Categories ADD Sort_Order INT NULL
END
GO

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
GO