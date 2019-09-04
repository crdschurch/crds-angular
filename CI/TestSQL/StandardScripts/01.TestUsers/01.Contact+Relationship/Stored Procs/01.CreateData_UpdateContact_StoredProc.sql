USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/15/2018
-- Description: Updates data on existing contact
-- Output:      @contact_id contains the contact id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Update_Contact
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Update_Contact')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Update_Contact
	@contact_email varchar(255),
	@middle_name nvarchar(50),
	@nickname varchar(50),
	@birthdate date,
	@gender_id int,
	@marital_status_id int,
	@prefix_id int,
	@household_position_id int,
	@mobile_phone_number nvarchar(25),
	@company_phone_number nvarchar(25),
	@contact_status_id int,
	@error_message nvarchar(500) OUTPUT,
	@contact_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Update_Contact] 
	@contact_email varchar(254),
	@middle_name nvarchar(50),
	@nickname varchar(50),
	@birthdate date,
	@gender_id int,
	@marital_status_id int,
	@prefix_id int,
	@household_position_id int,
	@mobile_phone_number nvarchar(25),
	@company_phone_number nvarchar(25),
	@contact_status_id int,
	@error_message nvarchar(500) OUTPUT,
	@contact_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @contact_email is null
	BEGIN
		SET @error_message = 'Contact email cannot be null'+CHAR(13);
		RETURN;
	END;
	

	--Required fields
	SET @contact_status_id = ISNULL(@contact_status_id, 1); --Active

	SET @contact_id = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @contact_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@contact_email+CHAR(13);
		RETURN;
	END;


	--Update contact
	UPDATE [dbo].Contacts
	SET Middle_Name = @middle_name,
	Nickname = @nickname,
	Date_of_Birth = @birthdate,
	Gender_ID = @gender_id,
	Marital_Status_ID = @marital_status_id,
	Prefix_ID = @prefix_id,
	Household_Position_ID = @household_position_id,
	Mobile_Phone = @mobile_phone_number,
	Company_Phone = @company_phone_number,
	Contact_Status_ID = @contact_status_id
	WHERE Contact_ID = @contact_id;
END
GO