USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/15/2018
-- Description: Creates contact and donor record for a new Guest Giver
-- Output:      @contact_id contains the contact id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Guest_Giver
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Guest_Giver')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Guest_Giver
	@contact_email varchar(254),
	@setup_date datetime,
	@processor_id nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@donor_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Guest_Giver] 
	@contact_email varchar(254),
	@setup_date datetime,
	@processor_id nvarchar(255),
	@error_message nvarchar(500) OUTPUT,
	@donor_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @contact_email is null
	BEGIN
		SET @error_message = 'Contact email cannot be null'+CHAR(13);
		RETURN;
	END;


	--Create Contact record
	DECLARE @contact_id int;
	DECLARE @throwaway_error nvarchar(500); --We don't expect the guest giver to exist, so the error message isn't useful and shouldn't be reported
	DECLARE @display_name nvarchar(75) = 'Guest Giver';
	EXEC [dbo].[cr_QA_Get_Contact_No_User_Acount] @display_name, @contact_email,
	@error_message = @throwaway_error OUTPUT, @contact_id = @contact_id OUTPUT;

	IF @contact_id is null
	BEGIN
		INSERT INTO [dbo].Contacts
		(Display_Name ,Email_Address ,Nickname     ,Company,Contact_Status_ID,Household_Position_ID,Bulk_Email_Opt_Out,Bulk_SMS_Opt_Out,Contact_GUID,Category_ID,Domain_ID) VALUES
		(@display_name,@contact_email,@display_name,0      ,1                ,1                    ,0                 ,0               ,NEWID()     ,2          ,1        );

		SET @contact_id = SCOPE_IDENTITY();
		IF @contact_id is null
		BEGIN
			SET @error_message = 'Could not create guest giver with email '+@contact_email+CHAR(13);
			RETURN;
		END;
	END;

	--Create Donor record
	SET @setup_date = ISNULL(@setup_date, GETDATE()); --Today

	DECLARE @statement_type_id int = 1; --Individual
	DECLARE @statement_frequency_id int = 3; --Never
	DECLARE @statement_method_id int = 4; --No statement needed

	EXEC [dbo].[cr_QA_Create_Donor_By_Contact_Id] @contact_id, @setup_date, @statement_type_id, @statement_frequency_id, @statement_method_id, @processor_id,
	@error_message = @error_message OUTPUT, @donor_id = @donor_id OUTPUT;	
END
GO