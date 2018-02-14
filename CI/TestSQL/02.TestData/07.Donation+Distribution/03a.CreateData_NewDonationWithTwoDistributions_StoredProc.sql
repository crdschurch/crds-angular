USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/01/2018
-- Description: Creates donation with two distributions
-- Output:      @donation_id contains the donation id, @error_message contains basic error message
-- =============================================

-- Defines cr_QA_New_Donation_With_Two_Distributions
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Donation_With_Two_Distributions')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Donation_With_Two_Distributions
	@donor_email nvarchar(254),
	@donation_amount money,
	@donation_date datetime,
	@payment_type_id int,
	@donation_status int,
	@receipted bit,
	@anonymous bit,
	@distribution1_amount money,
	@distribution1_program_name nvarchar(130),
	@distribution2_amount money,
	@distribution2_program_name nvarchar(130),
	@status_date datetime,
	@status_notes nvarchar(500),
	@processed bit,
	@batch_name nvarchar(75),
	@congregation_id int,
	@item_number nvarchar(15),
	@donation_notes nvarchar(500),
	@processor_id nvarchar(50),
	@transaction_code nvarchar(50),
	@pledge_user_email nvarchar(254),
	@error_message nvarchar(1500) OUTPUT,
	@donation_id int OUTPUT,
	@distribution1_id int OUTPUT,
	@distribution2_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Donation_With_Two_Distributions]
	@donor_email nvarchar(254),
	@donation_amount money,
	@donation_date datetime,
	@payment_type_id int,
	@donation_status int,
	@receipted bit,
	@anonymous bit,
	@distribution1_amount money,
	@distribution1_program_name nvarchar(130),
	@distribution2_amount money,
	@distribution2_program_name nvarchar(130),
	@status_date datetime,
	@status_notes nvarchar(500),
	@processed bit,
	@batch_name nvarchar(75),
	@congregation_id int,
	@item_number nvarchar(15),
	@donation_notes nvarchar(500),
	@processor_id nvarchar(50),
	@transaction_code nvarchar(50),
	@pledge_user_email nvarchar(254),
	@error_message nvarchar(1500) OUTPUT,
	@donation_id int OUTPUT,
	@distribution1_id int OUTPUT,
	@distribution2_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @donation_error nvarchar(500);
	EXEC [dbo].[cr_QA_New_Donation] @donor_email, @donation_amount, @donation_date, @payment_type_id, @donation_status, @receipted, @anonymous,
	@status_date, @status_notes, @processed, @batch_name, @item_number, @donation_notes, @processor_id, @transaction_code,
	@error_message = @donation_error OUTPUT, @donation_id = @donation_id OUTPUT;

	IF @donation_id is null
	BEGIN
		SET @error_message = @donation_error;
		RETURN;
	END;

	
	DECLARE @soft_credit_donor_id int = null; --No test data uses this yet, so pass in null
	DECLARE @distribution_notes nvarchar(1000) = null; --No test data uses this yet, so pass in null
	

	DECLARE @distribution1_error nvarchar(500);
	EXEC [dbo].[cr_QA_New_Donation_Distribution] @donation_id, @distribution1_amount, @distribution1_program_name, @pledge_user_email, @congregation_id, @soft_credit_donor_id, @distribution_notes, 
	@error_message = @distribution1_error OUTPUT, @distribution_id = @distribution1_id OUTPUT;
	
	DECLARE @distribution2_error nvarchar(500);
	EXEC [dbo].[cr_QA_New_Donation_Distribution] @donation_id, @distribution2_amount, @distribution2_program_name, @pledge_user_email, @congregation_id, @soft_credit_donor_id, @distribution_notes, 
	@error_message = @distribution2_error OUTPUT, @distribution_id = @distribution2_id OUTPUT;

	--Concatenate error message
	SET @error_message = @donation_error+@distribution1_error+@distribution2_error;
END
GO