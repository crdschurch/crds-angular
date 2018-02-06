USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/29/2018
-- Description:	Creates a new Payment (and Payment_Details) with given details
-- Output:      @payment_id contains the payment id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_New_Payment
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Payment')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Payment
	@user_email nvarchar(254),
	@payment_total money,
	@payment_date datetime,
	@congregation_id int,
	@batch_name nvarchar(75),
	@invoice_id int,
	@payment_type_id int,
	@transaction_code nvarchar(50),
	@error_message nvarchar(50) OUTPUT,
	@payment_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Payment]
	@user_email nvarchar(254),
	@payment_total money,
	@payment_date datetime,
	@congregation_id int,
	@batch_name nvarchar(75),
	@invoice_id int,
	@payment_type_id int,
	@transaction_code nvarchar(50),
	@error_message nvarchar(50) OUTPUT,
	@payment_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @user_email is null OR @invoice_id is null
	BEGIN
		SET @error_message = 'User email and invoice id cannot be null'
		RETURN;
	END;


	--Required fields
	SET @payment_date = ISNULL(@payment_date, GETDATE()); --Defaults to today
	SET @payment_total = ISNULL(@payment_total, 100);
	SET @congregation_id = ISNULL(@congregation_id, 5); --Not site specific

	DECLARE @payment_status_id int = 1; --Pending

	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @user_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+ @user_email;
		RETURN;
	END;

	DECLARE @invoice_detail_id int;
	SET @invoice_detail_id = (SELECT TOP 1 Invoice_Detail_ID FROM [dbo].Invoice_Detail where Invoice_ID = @invoice_id ORDER BY Invoice_Detail_ID ASC);
	IF @invoice_detail_id is null
	BEGIN
		SET @error_message = 'Could not find invoice detail on invoice'
		RETURN;
	END;


	--Optional fields
	SET @payment_type_id = ISNULL(@payment_type_id, 4); --Credit Card
	DECLARE @currency nvarchar(25) = 'USD';
	DECLARE @processed bit = 1;
	DECLARE @notes nvarchar(4000) = null;
	DECLARE @item_number nvarchar(15) = null;
	
	DECLARE @batch_id int = null;
	IF @batch_name is not null
	BEGIN
		SET @batch_id = (SELECT TOP 1 Batch_ID FROM [dbo].Batches WHERE Batch_Name = @batch_name ORDER BY Batch_ID ASC);
		IF @batch_id is null
			SET @error_message = 'Could not find batch with name '+@batch_name+' so it will not be added to the payment.'+CHAR(13);
	END;

	DECLARE @processor_fee_amount money = 0;
	IF @payment_type_id = 5 --Bank
		SET @processor_fee_amount = 0.25;
	IF @payment_type_id = 4 --Credit Card
	BEGIN
		SET @processor_fee_amount = ROUND(((ABS(@payment_total)/10)*0.2) + 0.25, 2); --Yes, really.
		IF @payment_total < 0
			SET @processor_fee_amount = @processor_fee_amount * -1; --Reimbursements
	END;	
	

	--Create Payment
	INSERT INTO [dbo].Payments
	(Payment_Total ,Contact_ID ,Domain_ID,Payment_Date ,Payment_Status_ID ,Transaction_Code ,Payment_Type_ID ,Invoice_Number,Batch_ID ,Processed ,Processor_Fee_Amount ,Currency ,Item_Number ,Notes ) VALUES
	(@payment_total,@contact_id,1        ,@payment_date,@payment_status_id,@transaction_code,@payment_type_id,@invoice_id   ,@batch_id,@processed,@processor_fee_amount,@currency,@item_number,@notes);

	SET @payment_id = SCOPE_IDENTITY();

	IF @payment_id is null
	BEGIN
		SET @error_message = @error_message+'Could not create payment for some reason';
		RETURN;
	END;


	--Create Payment Detail	
	INSERT INTO [dbo].Payment_Detail
	(Payment_ID ,Payment_Amount,Invoice_Detail_ID ,Domain_ID,Congregation_ID ) VALUES
	(@payment_id,@payment_total,@invoice_detail_id,1        ,@congregation_id);

	DECLARE @payment_detail_id int = SCOPE_IDENTITY();

	IF @payment_detail_id is null
		SET @error_message = @error_message+'Could not create payment detail'
END
GO