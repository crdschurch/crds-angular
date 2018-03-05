USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/01/2018
-- Description: Creates new Donation for donor with user account
-- Output:      @donation_id contains the donation id, @error_message contains basic error message
-- =============================================

-- Defines cr_QA_New_Donation
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Donation')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Donation
	@donor_email nvarchar(254),
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
	@non_cash_asset_type_id int,
	@error_message nvarchar(500) OUTPUT,
	@donation_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Donation]
	@donor_email nvarchar(254),
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
	@non_cash_asset_type_id int,
	@error_message nvarchar(500) OUTPUT,
	@donation_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @donor_email is null
	BEGIN
		SET @error_message = 'Donor email cannot be null'+CHAR(13);
		RETURN;
	END;

	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @donor_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@donor_email+CHAR(13);
		RETURN;
	END;
	
	--Create donation
	EXEC [dbo].[cr_QA_New_Donation_By_Contact_Id] @contact_id, @donation_amount, @donation_date, @payment_type_id, @donation_status, @receipted, @anonymous, @status_date, @status_notes,
	@processed, @batch_name, @item_number, @donation_notes, @processor_id, @transaction_code, @non_cash_asset_type_id,
	@error_message = @error_message OUTPUT, @donation_id = @donation_id OUTPUT;
END
GO