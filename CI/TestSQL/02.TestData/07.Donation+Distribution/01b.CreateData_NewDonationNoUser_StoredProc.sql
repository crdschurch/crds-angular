USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/01/2018
-- Description: Creates new Donation for a donor with no user account (guest givers, companies)
-- Output:      @donation_id contains the donation id, @error_message contains basic error message
-- =============================================

-- Defines cr_QA_New_Donation_No_User
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Donation_No_User')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Donation_No_User
	@display_name nvarchar(75),
	@is_company bit,
	@contact_email nvarchar(255),
	@donation_amount money,
	@donation_date datetime,
	@payment_type_id int,
	@donation_status int,
	@receipted bit,
	@anonymous bit,
	@status_date datetime,
	@status_notes nvarchar(500),
	@processed bit,
	@batch_name nvarchar(75),
	@item_number nvarchar(15),
	@donation_notes nvarchar(500),
	@processor_id nvarchar(50),
	@transaction_code nvarchar(50),
	@error_message nvarchar(500) OUTPUT,
	@donation_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Donation_No_User]
	@display_name nvarchar(75),
	@is_company bit,
	@contact_email nvarchar(255),
	@donation_amount money,
	@donation_date datetime,
	@payment_type_id int,
	@donation_status int,
	@receipted bit,
	@anonymous bit,
	@status_date datetime,
	@status_notes nvarchar(500),
	@processed bit,
	@batch_name nvarchar(75),
	@item_number nvarchar(15),
	@donation_notes nvarchar(500),
	@processor_id nvarchar(50),
	@transaction_code nvarchar(50),
	@error_message nvarchar(500) OUTPUT,
	@donation_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @display_name is null OR @is_company is null
	BEGIN
		SET @error_message = 'Display name and is company cannot be null'+CHAR(13);
		RETURN;
	END;
	
	--Required fields
	SET @donation_date = ISNULL(@donation_date, GETDATE()); --Today
	SET @donation_status = ISNULL(@donation_status, 1); --Pending
	SET @payment_type_id = ISNULL(@payment_type_id, 5); --Bank
	SET @receipted = ISNULL(@receipted, 0);

	--Get contact
	DECLARE @contact_id int;
	IF ISNULL(@is_company, 0) = 1
		IF @contact_email is not null
			SET @contact_id = (SELECT TOP 1 Contact_ID FROM [dbo].Contacts WHERE Display_Name = @display_name AND Company = 1 AND Email_Address = @contact_email ORDER BY Contact_ID ASC);
		ELSE
			SET @contact_id = (SELECT TOP 1 Contact_ID FROM [dbo].Contacts WHERE Display_Name = @display_name AND Company = 1 ORDER BY Contact_ID ASC);
	ELSE
	BEGIN
		IF @contact_email is not null
			SET @contact_id = (SELECT TOP 1 Contact_ID FROM [dbo].Contacts WHERE Display_Name = @display_name AND Company = 0 AND Email_Address = @contact_email ORDER BY Contact_ID ASC);
		ELSE --This may not get the contact you really want. Please include email address for more accuracy.
			SET @contact_id = (SELECT TOP 1 Contact_ID FROM [dbo].Contacts WHERE Display_Name = @display_name AND Company = 0 ORDER BY Contact_ID ASC); 
	END;

	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with display name and '+@display_name+' and (optional)email '+@contact_email+CHAR(13);
		RETURN;
	END;
	
	--Create donation
	EXEC [dbo].[cr_QA_New_Donation_By_Contact_Id] @contact_id, @donation_amount, @donation_date, @payment_type_id, @donation_status, @receipted, @anonymous, @status_date, @status_notes,
	@processed, @batch_name, @item_number, @donation_notes, @processor_id, @transaction_code,
	@error_message = @error_message OUTPUT, @donation_id = @donation_id OUTPUT;
END
GO