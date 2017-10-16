USE MinistryPlatform

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = N'Group_Inquiries')
   AND NOT EXISTS(select top 10 * from sys.columns  WHERE Name = N'Archived_Date' AND Object_ID = Object_ID(N'Group_Inquiries'))
BEGIN
	ALTER TABLE dbo.Group_Inquiries ADD Archived_Date DATETIME NULL;
END
GO