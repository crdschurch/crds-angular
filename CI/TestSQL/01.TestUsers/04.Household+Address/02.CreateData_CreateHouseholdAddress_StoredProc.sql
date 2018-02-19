USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/15/2018
-- Description: Create (if nonexistent) or Update Address record on a contact's household
-- Output:      @address_id contains the address id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Household_Address
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Household_Address')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Household_Address
	@household_member_email varchar(255),
	@line_1 nvarchar(75),
	@line_2 nvarchar(75),
	@city nvarchar(50),
	@state nvarchar(50),
	@zip nvarchar(15),
	@country nvarchar(50),
	@country_code nvarchar(25),
	@county nvarchar(50),
	@latitude nvarchar(15),
	@longitude nvarchar(15),
	@error_message nvarchar(500) OUTPUT,
	@address_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Household_Address] 
	@household_member_email varchar(255),
	@line_1 nvarchar(75),
	@line_2 nvarchar(75),
	@city nvarchar(50),
	@state nvarchar(50),
	@zip nvarchar(15),
	@country nvarchar(50),
	@country_code nvarchar(25),
	@county nvarchar(50),
	@latitude nvarchar(15),
	@longitude nvarchar(15),
	@error_message nvarchar(500) OUTPUT,
	@address_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @household_member_email is null OR @line_1 is null
	BEGIN
		SET @error_message = 'Household member email and address line 1 cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	DECLARE @contact_id int;
	SET @contact_id = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @household_member_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@household_member_email+CHAR(13);
		RETURN;
	END;
 
	DECLARE @household_id int = (SELECT Household_ID FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @household_id is null
	BEGIN
		--Use defaults
		EXEC [dbo].[cr_QA_Create_Household] @household_member_email, null, null,
		@error_message = @error_message OUTPUT, @household_id = @household_id OUTPUT;
		IF @household_id is null
		BEGIN
			SET @error_message = @error_message + 'Could not create household.'+CHAR(13);
			RETURN;
		END;
	END;

	--Create/Update address
	SET @address_id = (SELECT Address_ID FROM [dbo].Households WHERE Household_ID = @household_id);
	IF @address_id is null
	BEGIN
		INSERT INTO [dbo].Addresses
		(Address_Line_1,Domain_ID) VALUES
		(@line_1       ,1        );

		SET @address_id = SCOPE_IDENTITY();

		UPDATE [dbo].Households SET Address_ID = @address_id WHERE Household_ID = @household_id;
	END;
	
	IF @address_id is not null
	BEGIN
		UPDATE [dbo].Addresses
		SET Address_Line_1 = @line_1,
		Address_Line_2 = @line_2,
		City = @city,
		[State/Region] = @state,
		Postal_Code = @zip,
		Foreign_Country = @country,
		Country_Code = @country_code,
		County = @county,
		Latitude = @latitude,
		Longitude = @longitude
		WHERE Address_ID = @address_id;
	END;	
END
GO