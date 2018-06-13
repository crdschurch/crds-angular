USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 06/13/2018
-- Description:	Stored procedure declarations for deleting products.
-- =============================================


-- Defines cr_QA_Delete_Product_Option_Group
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Product_Option_Group')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Product_Option_Group
	@product_option_group_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Product_Option_Group] 
	@product_option_group_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @product_option_group_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	--Delete Product Option Prices (and nullify dependencies)
	DECLARE @product_option_prices TABLE
	(
		product_option_price_id int
	)
	INSERT INTO @product_option_prices (product_option_price_id) SELECT Product_Option_Price_ID 
		FROM [dbo].Product_Option_Prices WHERE Product_Option_Group_ID = @product_option_group_id;

	UPDATE [dbo].Invoice_Detail SET Product_Option_Price_ID = null WHERE Product_Option_Price_ID IN 
		(SELECT product_option_price_id FROM @product_option_prices);

	DELETE [dbo].Product_Option_Prices WHERE Product_Option_Price_ID IN 
		(SELECT product_option_price_id FROM @product_option_prices);
				
	DELETE [dbo].Product_Option_Groups WHERE Product_Option_Group_ID = @product_option_group_id;
END
GO


-- Defines cr_QA_Delete_Product
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Product')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Product
	@product_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Product] 
	@product_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @product_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Product_Ruleset WHERE Product_ID = @product_id;
	DELETE [dbo].Product_Option_Groups WHERE Product_ID = @product_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Invoice Details
	DECLARE @invoice_details_to_delete TABLE
	(
		invoice_detail_id int
	)
	INSERT INTO @invoice_details_to_delete (invoice_detail_id) SELECT Invoice_Detail_ID 
		FROM [dbo].Invoice_Detail WHERE Product_ID = @product_id;

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

	--Delete Product Option Groups
	DECLARE @product_option_groups_to_delete TABLE
	(
		product_option_group_id int
	)
	INSERT INTO @product_option_groups_to_delete (product_option_group_id) SELECT Product_Option_Group_ID 
		FROM [dbo].Product_Option_Groups WHERE Product_ID = @product_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 product_option_group_id
			FROM @product_option_groups_to_delete
			WHERE product_option_group_id > @cur_entry_id
			ORDER BY product_option_group_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Product_Option_Group]  @cur_entry_id;
		END
	END
	
	--Nullify foreign keys
	UPDATE [dbo].Events SET Online_Registration_Product = null WHERE Online_Registration_Product = @product_id;

	DELETE [dbo].Products WHERE Product_ID = @product_id;
END
GO


-- Defines cr_QA_Delete_Product_By_Name
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Product_By_Name')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Product_By_Name
	@product_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Product_By_Name] 
	@product_name int
AS
BEGIN
	SET NOCOUNT ON;

	IF @product_name is null
		RETURN;

	--Delete Products by name
	DECLARE @products_to_delete TABLE
	(
		product_id int
	)
	INSERT INTO @products_to_delete (product_id) SELECT Product_ID FROM [dbo].Products 
	WHERE Product_Name = @product_name;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 product_id 
			FROM @products_to_delete
			WHERE product_id > @cur_entry_id
			ORDER BY product_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Product] @cur_entry_id;
		END
	END
END
GO