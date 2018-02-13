USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/02/2018
-- Description: Creates a new Invoice and makes one Payment towards it
-- Output:      @invoice_id contains the invoice id, @invoice_detail_id contains the invoice detail id,
--              @payment_id contains the payment id, @payment_detail_id contains the payment detail id,
--              @error_message contains basic error message
-- =============================================


-- Defines cr_QA_New_Invoice_With_Payment
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Invoice_With_Payment')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Invoice_With_Payment
	@user_email nvarchar(254),
	@invoice_total money,
	@invoice_date datetime,
	@product_name nvarchar(50),
	@batch_name nvarchar(75),
	@payment_total money,
	@payment_date datetime,
	@payment_type_id int,
	@payment_transaction_code nvarchar(50),
	@congregation_id int,
	@error_message nvarchar(1000) OUTPUT,
	@invoice_id int OUTPUT,
	@invoice_detail_id int OUTPUT,
	@payment_id int OUTPUT,
	@payment_detail_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Invoice_With_Payment]
	@user_email nvarchar(254),
	@invoice_total money,
	@invoice_date datetime,
	@product_name nvarchar(50),
	@batch_name nvarchar(75),
	@payment_total money,
	@payment_date datetime,
	@payment_type_id int,
	@payment_transaction_code nvarchar(50),
	@congregation_id int,
	@error_message nvarchar(1000) OUTPUT,
	@invoice_id int OUTPUT,
	@invoice_detail_id int OUTPUT,
	@payment_id int OUTPUT,
	@payment_detail_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @invoice_error nvarchar(500);
	EXECUTE [dbo].[cr_QA_New_Invoice] @user_email, @invoice_total, @invoice_date, @product_name,
	@error_message = @invoice_error OUTPUT, @invoice_id = @invoice_id OUTPUT, @invoice_detail_id = @invoice_detail_id OUTPUT

	IF @invoice_id is null OR @invoice_detail_id is null
	BEGIN
		SET @error_message = @invoice_error;
		RETURN;
	END;

	DECLARE @payment_error nvarchar(500);
	EXECUTE [dbo].[cr_QA_New_Payment] @user_email, @payment_total, @payment_date, @congregation_id, @batch_name, @invoice_id, @payment_type_id, @payment_transaction_code,
	@error_message = @payment_error OUTPUT, @payment_id = @payment_id OUTPUT, @payment_detail_id = @payment_detail_id OUTPUT;

	--Concatenate error messages
	SET @error_message = @invoice_error+@payment_error;
END
GO