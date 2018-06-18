USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/15/2018
-- Description: Creates (if nonexistent) or Updates household
-- Output:      @household_id contains the household id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Household
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Household')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Household
	@household_member_email varchar(254),
	@home_phone_number nvarchar(25),
	@congregation_id int,
	@error_message nvarchar(500) OUTPUT,
	@household_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Household] 
	@household_member_email varchar(254),
	@home_phone_number nvarchar(25),
	@congregation_id int,
	@error_message nvarchar(500) OUTPUT,
	@household_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Required fields
	DECLARE @bulk_mail_opt_out bit = 0;
	DECLARE @repeats_annually bit = 0;

	IF @household_member_email is null
	BEGIN
		SET @error_message = 'Household member email cannot be null'+CHAR(13);
		RETURN;
	END;
	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @household_member_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@household_member_email+CHAR(13);
		RETURN;
	END;
	
	DECLARE @household_name varchar(255);
	SET @household_name = ISNULL((SELECT Last_Name FROM [dbo].Contacts WHERE Contact_ID = @contact_id), 'Smith'); --This will only be used when creating a household
	

	--Optional fields
	SET @congregation_id = ISNULL(@congregation_id, 15); --Anywhere


	--Create/Update household
	SET @household_id = (SELECT Household_ID FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @household_id is null
	BEGIN
		INSERT INTO [dbo].Households
		(Household_Name ,Domain_ID,Bulk_Mail_Opt_Out ,Repeats_Annually ) VALUES
		(@household_name,1        ,@bulk_mail_opt_out,@repeats_annually);

		SET @household_id = SCOPE_IDENTITY();

		UPDATE [dbo].Contacts SET Household_ID = @household_id WHERE Contact_ID = @contact_id;
	END;
	
	IF @household_id is not null
	BEGIN
		UPDATE [dbo].Households
		SET Home_Phone = @home_phone_number,
		Congregation_ID = @congregation_id,
		Bulk_Mail_Opt_Out = @bulk_mail_opt_out,
		Repeats_Annually = @repeats_annually
		WHERE Household_ID = @household_id;
	END;
END
GO