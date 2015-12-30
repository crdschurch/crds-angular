USE [MinistryPlatform]
GO

IF NOT EXISTS(SELECT *
         FROM   INFORMATION_SCHEMA.COLUMNS
         WHERE  TABLE_NAME = 'Congregations'
                AND COLUMN_NAME = 'Childcare_Contact')
BEGIN
	ALTER Table [dbo].[Congregations] ADD Childcare_Contact int null
END

IF NOT EXISTS (SELECT * 
          FROM sys.foreign_keys 
          WHERE object_id = OBJECT_ID(N'[dbo].[FK_CONGREGATIONS_CHILDCARE_CONTACT]') 
            AND parent_object_id = OBJECT_ID(N'[dbo].[Congregations]'))
BEGIN
	Alter Table [dbo].[Congregations] WITH CHECK ADD CONSTRAINT [FK_CONGREGATIONS_CHILDCARE_CONTACT] 
		FOREIGN KEY(Childcare_Contact) references [dbo].[Contacts](Contact_ID)
END

UPDATE [dbo].[dp_Pages] 
	SET DEFAULT_FIELD_LIST = N' Congregations.Congregation_Name
							   ,Location_ID_Table.Location_Name
							   ,Congregations.Start_Date
								,Contact_ID_Table.Display_Name AS Contact_Person
								,Pastor_Table.Display_Name AS Pastor
								,Congregations.End_Date
								,Childcare_Contact_Table.[Display_Name] AS [Childcare_Contact]'
	WHERE PAGE_ID = 288
GO
