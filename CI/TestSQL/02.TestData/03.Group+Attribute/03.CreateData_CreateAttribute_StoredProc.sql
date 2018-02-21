USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/05/2018
-- Description: Creates (if nonexistent) or Updates an attribute
-- Output:      @attribute_id contains the attribute id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Attribute
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Attribute')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Attribute
	@attribute_name nvarchar(100),
	@attribute_type_id int,
	@attribute_category_id int,
	@error_message nvarchar(500) OUTPUT,
	@attribute_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Attribute] 
	@attribute_name nvarchar(100),
	@attribute_type_id int,
	@attribute_category_id int,
	@error_message nvarchar(500) OUTPUT,
	@attribute_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	--Enforce required parameters
	IF @attribute_name is null
	BEGIN
		SET @error_message = 'Attribute name cannot be null'+CHAR(13);
		RETURN;
	END;


	--Required fields
	SET @attribute_type_id = ISNULL(@attribute_type_id, 90); --Group category

	DECLARE @sort_order int = 0;


	--Optional fields
	DECLARE @description nvarchar(255) = null;

	
	--Create/Edit attribute
	SET @attribute_id = (SELECT TOP 1 Attribute_ID FROM [dbo].Attributes WHERE Attribute_Name = @attribute_name ORDER BY Attribute_ID ASC);
	IF @attribute_id is null
	BEGIN
		INSERT INTO [dbo].Attributes 
		(Attribute_Name ,Attribute_Type_ID ,Sort_Order ,Domain_ID) VALUES
		(@attribute_name,@attribute_type_id,@sort_order,1        );

		SET @attribute_id = SCOPE_IDENTITY();
	END;

	IF @attribute_id is not null
	BEGIN
		UPDATE [dbo].Attributes
		SET Attribute_Type_ID = @attribute_type_id,
		Sort_Order = @sort_order,
		Attribute_Category_ID = @attribute_category_id,
		Description = @description
		WHERE Attribute_ID = @attribute_id;
	END;
END
GO