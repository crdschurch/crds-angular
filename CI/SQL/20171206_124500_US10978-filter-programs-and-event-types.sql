USE [MinistryPlatform]
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Programs' AND COLUMN_NAME = 'Show_On_Event_Tool')
BEGIN
    ALTER TABLE Programs ADD Show_On_Event_Tool BIT NOT NULL DEFAULT(0);
END
GO

-- Start with full list to match current state; manual tuning will happen later
UPDATE Programs SET Show_On_Event_Tool = 1 WHERE End_Date IS NULL OR End_Date > GETDATE();
GO


IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Event_Types' AND COLUMN_NAME = 'Show_On_Event_Tool')
BEGIN
    ALTER TABLE Event_Types ADD Show_On_Event_Tool BIT NOT NULL DEFAULT(0);
END
GO

-- Start with full list to match current state; manual tuning will happen later
UPDATE Event_Types SET Show_On_Event_Tool = 1;
GO
