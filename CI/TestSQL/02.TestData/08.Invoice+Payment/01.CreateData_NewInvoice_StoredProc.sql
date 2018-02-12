USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 02/02/2018
-- Description:	Creates a new Invoice (and Invoice_Details) with given details
-- Output:      @invoice_id contains the invoice id, @invoice_detail_id contains the invoice detail id,
--              @error_message contains basic error message
-- =============================================


-- Defines cr_QA_New_Invoice
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_New_Invoice')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_New_Invoice
	@purchaser_email nvarchar(254),
	@invoice_total money,
	@invoice_date datetime,
	@product_name nvarchar(50),
	@error_message nvarchar(500) OUTPUT,
	@invoice_id int OUTPUT,
	@invoice_detail_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_New_Invoice]
	@purchaser_email nvarchar(254),
	@invoice_total money,
	@invoice_date datetime,
	@product_name nvarchar(50),
	@error_message nvarchar(500) OUTPUT,
	@invoice_id int OUTPUT,
	@invoice_detail_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @purchaser_email is null OR @product_name is null
	BEGIN
		SET @error_message = 'Purchaser email and product name cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @invoice_total = ISNULL(@invoice_total, 500);
	SET @invoice_date = ISNULL(@invoice_date, GETDATE()); --Defaults to today
	
	DECLARE @invoice_status_id int = 2; --Some paid
	DECLARE @item_quantity int = 1; --This is always 1. Always.

	DECLARE @product_id int;
	SET @product_id = (SELECT TOP 1 Product_ID FROM [dbo].Products WHERE Product_Name = @product_name ORDER BY Product_ID ASC);
	IF @product_id is null
	BEGIN
		SET @error_message = 'Could not find product with name '+@product_name+CHAR(13);
		RETURN;
	END;

	DECLARE @purchaser_contact_id INT = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @purchaser_email);
	IF @purchaser_contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@purchaser_email+CHAR(13);
		RETURN;
	END;


	--Optional fields
	DECLARE @currency nvarchar(25) = 'USD';
	DECLARE @form_response_id int = null;
	DECLARE @notes nvarchar(4000) = null;


	--Create Invoice
	INSERT INTO [dbo].Invoices
	(Purchaser_Contact_ID ,Invoice_Status_ID ,Invoice_Total ,Invoice_Date ,Domain_ID,Form_Response_ID ,Currency ,Notes ) VALUES
	(@purchaser_contact_id,@invoice_status_id,@invoice_total,@invoice_date,1        ,@form_response_id,@currency,@notes);

	SET @invoice_id = SCOPE_IDENTITY();

	IF @invoice_id is null
	BEGIN
		SET @error_message = 'Could not create invoice for some reason'+CHAR(13);
		RETURN;
	END;

	
	--Create Invoice Details
	INSERT INTO [dbo].Invoice_Detail
	(Invoice_ID ,Recipient_Contact_ID ,Item_Quantity ,Line_Total    ,Product_ID ,Domain_ID) VALUES
	(@invoice_id,@purchaser_contact_id,@item_quantity,@invoice_total,@product_id,1        );

	SET @invoice_detail_id = SCOPE_IDENTITY();
END
GO