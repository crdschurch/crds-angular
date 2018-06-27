USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/05/2018
-- Description: Creates (if nonexistent) or Updates group attribute
-- Output:      @group_attribute_id contains the group attribute id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Group_Attribute
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Group_Attribute')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Group_Attribute
	@attribute_name nvarchar(100),
	@group_name nvarchar(75),
	@start_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@group_attribute_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Group_Attribute] 
	@attribute_name nvarchar(100),
	@group_name nvarchar(75),
	@start_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@group_attribute_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	--Required fields
	SET @start_date = ISNULL(@start_date, GETDATE());

	IF @attribute_name is null
	BEGIN
		SET @error_message = 'Attribute name cannot be null'+CHAR(13);
		RETURN;
	END;
	DECLARE @attribute_id int = (SELECT TOP 1 Attribute_ID FROM [dbo].Attributes WHERE Attribute_Name = @attribute_name ORDER BY Attribute_ID ASC);
	IF @attribute_id is null
	BEGIN
		SET @error_message = 'Attribute with name '+@attribute_name+' could not be found'+CHAR(13);
		RETURN;
	END;
	
	IF @group_name is null
	BEGIN
		SET @error_message = 'Group name cannot be null'+CHAR(13);
		RETURN;
	END;
	DECLARE @group_id int = (SELECT TOP 1 Group_ID FROM [dbo].Groups WHERE Group_Name = @group_name ORDER BY Group_ID ASC);
	IF @group_id is null
	BEGIN
		SET @error_message = 'Group with name '+@group_name+' could not be found'+CHAR(13);
		RETURN;
	END;

	
	--Create/Update group attribute
	SET @group_attribute_id = (SELECT TOP 1 Attribute_ID FROM [dbo].Group_Attributes WHERE Attribute_ID = @attribute_id AND Group_ID = @group_id ORDER BY Group_Attribute_ID ASC);
	IF @group_attribute_id is null
	BEGIN
		INSERT INTO [dbo].Group_Attributes 
		(Attribute_ID ,Group_ID ,Start_Date ,Domain_ID) VALUES
		(@attribute_id,@group_id,@start_date,1        );

		SET @group_attribute_id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [dbo].Group_Attributes
		SET Start_Date = @start_date
		WHERE Group_Attribute_ID = @group_attribute_id;
	END;
END
GO