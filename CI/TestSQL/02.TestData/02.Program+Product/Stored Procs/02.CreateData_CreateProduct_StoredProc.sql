USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 06/13/2018
-- Description: Creates (if nonexistent) or Updates product information
-- Output:      @product_id contains the product id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Product
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Product')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Product
	@product_name nvarchar(50),
	@base_price money,
	@deposit_price money,
	@program_name nvarchar(130),
	@error_message nvarchar(500) OUTPUT,
	@product_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Product] 
	@product_name nvarchar(50),
	@base_price money,
	@deposit_price money,
	@program_name nvarchar(130),
	@error_message nvarchar(500) OUTPUT,
	@product_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @product_name is null
	BEGIN
		SET @error_message = 'Product name cannot be null'+CHAR(13);
		RETURN;
	END;

	--Required fields
	SET @base_price = ISNULL(@base_price, 250.55);

	DECLARE @active bit = 1;

	--Optional fields
	DECLARE @price_currency int = 1; --USD
	DECLARE @description varchar(MAX) = null;

	DECLARE @program_id int = null;
	IF @program_name is not null
	BEGIN
		SET @program_id = (SELECT TOP 1 Program_ID FROM [dbo].Programs WHERE Program_Name = @program_name ORDER BY Program_ID ASC);
		IF @program_id is null
			SET @error_message = 'Could not find program with name '+@program_name+' so it will not be added to the product.'+CHAR(13);
	END;
		
	--Create/Edit product
	SET @product_id = (SELECT TOP 1 Product_ID FROM [dbo].Products WHERE Product_Name = @product_name ORDER BY Product_ID ASC);
	IF @product_id is null
	BEGIN
		INSERT INTO [dbo].Products 
		(Product_Name ,Base_Price ,Active ,Domain_ID) VALUES
		(@product_name,@base_price,@active,1        );

		SET @product_id = SCOPE_IDENTITY();
	END;

	IF @product_id is not null
	BEGIN
		UPDATE [dbo].Products
		SET Base_Price = @base_price,
		Deposit_Price = @deposit_price,
		Program_ID = @program_id,
		Description = cast(@description as text)
		WHERE Product_ID = @product_id;
	END
	ELSE
	BEGIN
		SET @error_message = @error_message+'Could not create product for some reason'+CHAR(13);
		RETURN;
	END;
END
GO