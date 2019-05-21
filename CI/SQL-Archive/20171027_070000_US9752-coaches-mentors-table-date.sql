USE MinistryPlatform
GO

   
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = N'cr_Coaches')
BEGIN
  ALTER TABLE dbo.cr_Coaches
   ALTER COLUMN Start_Date date NOT NULL

  ALTER TABLE dbo.cr_Coaches
   ALTER COLUMN End_Date date NULL
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = N'cr_Mentors')
BEGIN
  ALTER TABLE dbo.cr_Mentors
   ALTER COLUMN Start_Date date NOT NULL

  ALTER TABLE dbo.cr_Mentors
   ALTER COLUMN End_Date date NULL
END

GO
