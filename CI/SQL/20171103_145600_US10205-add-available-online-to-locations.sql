USE MinistryPlatform
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'Locations' AND COLUMN_NAME = 'Available_Online')
BEGIN
	ALTER TABLE dbo.Locations ADD Available_Online BIT NULL;
END
GO

UPDATE Locations
SET Available_Online = 1
WHERE Location_Name = 'Oakley' 
OR Location_Name = 'Mason'
OR Location_Name = 'Florence' 
OR Location_Name = 'West Side' 
OR Location_Name = 'Uptown' 
OR Location_Name = 'Anywhere' 
OR Location_Name = 'Oxford' 
OR Location_Name = 'Georgetown' 
OR Location_Name = 'Richmond' 
OR Location_Name = 'Andover' 
OR Location_Name = 'Columbus' 
OR Location_Name = 'Downtown Lexington' 
