USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 02/01/2018
-- Description:	Creates donation and distribution with given information
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
	@error_message nvarchar(500) OUTPUT,
	@donation_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @donor_email is null OR @donation_amount is null
	BEGIN
		SET @error_message = 'User email and amount cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @donation_date = ISNULL(@donation_date, GETDATE()); --Today
	SET @donation_status = ISNULL(@donation_status, 1); --Pending
	SET @payment_type_id = ISNULL(@payment_type_id, 5); --Bank
	SET @receipted = ISNULL(@receipted, 0);

	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @donor_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@donor_email+CHAR(13);
		RETURN;
	END;

	DECLARE @donor_id int = (SELECT Donor_Record FROM [dbo].Contacts WHERE Contact_ID = @contact_id);
	IF @donor_id is null
	BEGIN
		DECLARE @statement_frequency int = 1; --Quarterly
		DECLARE @statement_method int = 1; --Postal
		DECLARE @statement_type int = 1; --Individual

		INSERT INTO [dbo].Donors
		(Contact_ID ,Setup_Date,Statement_Frequency_ID,Statement_Method_ID,Statement_Type_ID,Domain_ID) VALUES
		(@contact_id,GETDATE() ,@statement_frequency  ,@statement_method  ,@statement_type  ,1        );

		SET @donor_id = SCOPE_IDENTITY();
	END;

	
	--Optional fields
	SET @anonymous = ISNULL(@anonymous, 0);
	SET @status_date = ISNULL(@status_date, @donation_date);
	
	DECLARE @currency nvarchar(25) = 'USD';
	DECLARE @is_registered_donor bit = 1; --Is not guest giver
	DECLARE @invoice_number nvarchar(25) = null;
	DECLARE @is_recurring_gift bit = null;
	DECLARE @recurring_gift_id int = null;
	DECLARE @non_cash_asset_id int = null;

	DECLARE @processor_fee_amount money = 0;
	IF @payment_type_id = 5 --Bank
		SET @processor_fee_amount = 0.25;
	IF @payment_type_id = 4 --Credit Card
	BEGIN
		SET @processor_fee_amount = ROUND(((ABS(@donation_amount)/10)*0.2) + 0.25, 2); --Yes, really.
		IF @donation_amount < 0
			SET @processor_fee_amount = @processor_fee_amount * -1; --Reimbursements
	END;
	
	DECLARE @batch_id int = null;
	DECLARE @batch_position int = null;
	IF @batch_name is not null
	BEGIN
		SET @batch_id = (SELECT TOP 1 Batch_ID FROM [dbo].Batches WHERE Batch_Name = @batch_name ORDER BY Batch_ID ASC);
		IF @batch_id is null
			SET @error_message = 'Could not find batch with name '+@batch_name+', so will not be added to donation'+CHAR(13);
		ELSE
			SET @batch_position = (SELECT COUNT(Donation_ID) FROM [dbo].Donations WHERE Batch_ID = @batch_id) + 1;
	END;
	

	--Create Donation
	INSERT INTO [dbo].Donations
	(Donor_ID ,Donation_Amount ,Donation_Date ,Payment_Type_ID ,Notes          ,Batch_ID  ,Donation_Status_ID,Donation_Status_Date,Domain_ID,Currency ,Processed ,Receipted ,Position       ,Anonymous ,Donation_Status_Notes,Invoice_Number ,Is_Recurring_Gift ,Item_Number ,Non_Cash_Asset_Type_ID,Processor_Fee_Amount ,Processor_ID ,Recurring_Gift_ID ,Registered_Donor    ,Transaction_Code ) VALUES
	(@donor_id,@donation_amount,@donation_date,@payment_type_id,@donation_notes,@batch_id ,@donation_status  ,@status_date        ,1        ,@currency,@processed,@receipted,@batch_position,@anonymous,@status_notes        ,@invoice_number,@is_recurring_gift,@item_number,@non_cash_asset_id    ,@processor_fee_amount,@processor_id,@recurring_gift_id,@is_registered_donor,@transaction_code);

	SET @donation_id = SCOPE_IDENTITY();
END
GO