USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting invoice and payment data and their details.
-- =============================================


-- Defines cr_QA_Delete_Payment
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Payment')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Payment
	@payment_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Payment] 
	@payment_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @payment_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Payment_Detail WHERE Payment_ID = @payment_id;

	DELETE [dbo].Payments WHERE Payment_ID = @payment_id;
END
GO


-- Defines cr_QA_Delete_Invoice_Detail
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Invoice_Detail')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Invoice_Detail
	@invoice_detail_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Invoice_Detail] 
	@invoice_detail_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @invoice_detail_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Payment_Detail WHERE Invoice_Detail_ID = @invoice_detail_id;

	DELETE [dbo].Invoice_Detail WHERE Invoice_Detail_ID = @invoice_detail_id;
END
GO


-- Defines cr_QA_Delete_Invoice
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Invoice')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Invoice
	@invoice_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Invoice]
	@invoice_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @invoice_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Invoice Details
	DECLARE @invoice_details_to_delete TABLE
	(
		invoice_detail_id int
	)
	INSERT INTO @invoice_details_to_delete (invoice_detail_id) SELECT Invoice_Detail_ID 
		FROM [dbo].Invoice_Detail WHERE Invoice_ID = @invoice_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 invoice_detail_id 
			FROM @invoice_details_to_delete
			WHERE invoice_detail_id > @cur_entry_id
			ORDER BY invoice_detail_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Invoice_Detail] @cur_entry_id;
		END
	END
	
	--Nullify foreign keys
	UPDATE [dbo].Form_Responses SET Invoice_ID = null WHERE Invoice_ID = @invoice_id;

	DELETE [dbo].Invoices WHERE Invoice_ID = @invoice_id;
END
GO