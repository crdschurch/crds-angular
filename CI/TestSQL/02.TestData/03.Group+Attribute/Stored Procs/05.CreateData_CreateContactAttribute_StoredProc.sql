USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 02/05/2018
-- Description: Creates (if nonexistent) or Updates contact attribute
-- Output:      @contact_attribute_id contains the contact attribute id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Contact_Attribute
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Contact_Attribute')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Contact_Attribute
	@attribute_name nvarchar(100),
	@contact_email varchar(255),
	@start_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@contact_attribute_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Contact_Attribute]
	@attribute_name nvarchar(100),
	@contact_email varchar(255),
	@start_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@contact_attribute_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	--Enforce required parameters
	IF @attribute_name is null OR @contact_email is null
	BEGIN
		SET @error_message = 'Attribute name and contact email cannot be null'+CHAR(13);
		RETURN;
	END;
	

	--Required fields
	SET @start_date = ISNULL(@start_date, GETDATE());

	DECLARE @attribute_id int = (SELECT TOP 1 Attribute_ID FROM [dbo].Attributes WHERE Attribute_Name = @attribute_name ORDER BY Attribute_ID ASC);
	IF @attribute_id is null
	BEGIN
		SET @error_message = 'Attribute with name '+@attribute_name+' could not be found'+CHAR(13);
		RETURN;
	END;
	
	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @contact_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@contact_email+CHAR(13);
		RETURN;
	END;

	
	--Create/Update group attribute
	SET @contact_attribute_id = (SELECT TOP 1 Contact_Attribute_ID FROM [dbo].Contact_Attributes WHERE Attribute_ID = @attribute_id AND Contact_ID = @contact_id ORDER BY Contact_Attribute_ID ASC);
	IF @contact_attribute_id is null
	BEGIN
		INSERT INTO [dbo].Contact_Attributes 
		(Contact_ID ,Attribute_ID ,Start_Date ,Domain_ID) VALUES
		(@contact_id,@attribute_id,@start_date,1        );

		SET @contact_attribute_id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [dbo].Contact_Attributes
		SET Start_Date = @start_date
		WHERE Contact_Attribute_ID = @contact_attribute_id;
	END;
END
GO