/****** Script for SelectTopNRows command from SSMS  ******/
SELECT c.First_Name, c.Middle_Name, c.Last_Name, c.Maiden_Name, c.Nickname, c.Email_Address, 
c.Date_of_Birth,
c.Marital_Status_ID, c.Gender_ID,
c.Employer_Name, c.Mobile_Phone
FROM [MinistryPlatform].[dbo].[Contacts] c
where c.Contact_ID = 768379