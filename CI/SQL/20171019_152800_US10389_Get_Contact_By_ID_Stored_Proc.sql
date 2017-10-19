USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[api_crds_Get_Contact_By_ID]    Script Date: 10/19/2017 12:40:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Ken Baum
-- Create date: 10/18/2017
-- Description:	Return data on a contact by ID
-- =============================================
ALTER PROCEDURE [dbo].[api_crds_Get_Contact_By_ID]
	@ContactID int
AS
BEGIN

	select  c.[__Age] AS [Age],
			c.Contact_ID,
			c.Date_of_Birth ,
			c.Gender_ID,
			c.Marital_Status_ID,
			c.Display_Name , 
			c.Email_Address ,
			c.Employer_Name ,
			c.First_Name ,
			c.Current_School,
			c.Last_Name ,
			c.Maiden_Name ,
			c.Middle_Name ,
			c.Mobile_Phone ,
			c.Nickname ,
			c.[Passport_Country] ,
			c.[Passport_Expiration] ,
			c.[Passport_Firstname] ,
			c.[Passport_Lastname] ,
			c.[Passport_Middlename] ,
			c.[Passport_Number],
			c.Mobile_Carrier as Mobile_Carrier_ID,
			a.Address_ID ,
			a.Address_Line_1 ,
			a.Address_Line_2 ,
			a.City,
			a.County ,
			a.Foreign_Country ,
			a.Postal_Code ,
			a.[State/Region] AS State, 
			h.Home_Phone ,
			h.Household_ID ,
			h.Household_Name ,
			h.Congregation_ID,
			p.Participant_Start_Date,
			p.Attendance_Start_Date		
	from dbo.Contacts c
		inner join dbo.Households h on c.Household_ID = h.Household_ID
		inner join dbo.Addresses a on h.Address_ID = a.Address_ID
		inner join dbo.Participants p on p.Contact_ID = c.Contact_ID
	where c.Contact_ID = @ContactID

END
