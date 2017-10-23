USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[api_crds_Get_Contact_By_ID]    Script Date: 10/19/2017 11:27:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.api_crds_Get_Contact_By_ID') IS NULL -- Check if SP Exists
        EXEC('CREATE PROCEDURE dbo.api_crds_Get_Contact_By_ID AS SET NOCOUNT ON;') -- Create dummy/empty SP
GO


-- =============================================
-- Author:		Ken Baum
-- Create date: 10/18/2017
-- Description:	Return contact data given a contact ID
-- =============================================
ALTER PROCEDURE [dbo].[api_crds_Get_Contact_By_ID]
	@ContactID int
AS
BEGIN

	SET NOCOUNT ON;

	select  COALESCE(c.[__Age],0) AS [Age],
			c.Contact_ID,
			COALESCE(CONVERT(varchar, c.Date_of_Birth, 101), '') as Date_Of_Birth,
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
			COALESCE(h.Household_ID,0) as Household_ID,
			h.Household_Name ,
			h.Congregation_ID,
			p.Participant_Start_Date,
			p.Attendance_Start_Date		
	from dbo.Contacts c
		left outer join dbo.Households h on c.Household_ID = h.Household_ID
		left outer  join dbo.Addresses a on h.Address_ID = a.Address_ID
		left outer  join dbo.Participants p on p.Contact_ID = c.Contact_ID
	where c.Contact_ID = @ContactID


END
GO

-- setup permissions for API User in MP

DECLARE @procName nvarchar(100) = N'api_crds_Get_Contact_By_ID'
DECLARE @procDescription nvarchar(100) = N'Retrieves information on a contact given the ID.'

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = @procName)
BEGIN
        INSERT INTO [dbo].[dp_API_Procedures] (
                 Procedure_Name
                ,Description
        ) VALUES (
                 @procName
                ,@procDescription
        )
END

DECLARE @API_ROLE_ID int = 62;
DECLARE @API_ID int;

SELECT @API_ID = API_Procedure_ID FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = @procName;

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_Role_API_Procedures] WHERE [Role_ID] = @API_ROLE_ID AND [API_Procedure_ID] = @API_ID)
BEGIN
        INSERT INTO [dbo].[dp_Role_API_Procedures] (
                 [Role_ID]
                ,[API_Procedure_ID]
                ,[Domain_ID]
        ) VALUES (
                 @API_ROLE_ID
                ,@API_ID
                ,1
        )
END
GO