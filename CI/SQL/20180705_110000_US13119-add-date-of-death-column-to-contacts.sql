USE [MinistryPlatform]
GO

/****** Object:  Table [dbo].[Contacts]    Script Date: 7/5/2018 11:14:21 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

ALTER TABLE dbo.Contacts
ADD Date_Of_Death date null;
GO

UPDATE [dbo].[dp_Pages]
   SET [Default_Field_List] = 'Contacts.Display_Name ,Contacts.Nickname ,Contacts.First_Name ,Contacts.Last_Name ,Contact_Status_ID_Table.Contact_Status ,
								Household_ID_Table.Home_Phone ,Contacts.Mobile_Phone ,Household_ID_Table_Address_ID_Table.Address_Line_1 ,
								Household_ID_Table_Address_ID_Table.City ,Household_ID_Table_Address_ID_Table.[State/Region] AS State ,Household_ID_Table_Address_ID_Table.Postal_Code ,
								Contacts.Email_Address ,Convert(Varchar(12),Contacts.Date_of_Birth,101) AS Date_of_Birth  ,Convert(Varchar(12),Contacts.Date_of_Death,101) AS Date_of_Death ,
								Gender_ID_Table.Gender ,Marital_Status_ID_Table.Marital_Status ,Household_ID_Table_Congregation_ID_Table.Congregation_Name ,Household_ID_Table.Household_Name ,
								Household_Position_ID_Table.Household_Position, Category_ID_Table.Description AS Contact_Category'
WHERE Page_ID = 292
GO