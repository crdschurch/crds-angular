USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:      Henney, Sarah
-- Create date: 01/16/2018
-- Description: Creates (if non-existant) or Updates contact relationship between contacts
-- Output:      @contact_relationship_id contains the contact relationship id, @error_message contains basic error message
-- =============================================


-- Defines cr_QA_Create_Contact_Relationship
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Create_Contact_Relationship')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Create_Contact_Relationship
	@contact_email varchar(254),
	@related_contact_email varchar(254),
	@relationship_id int,
	@start_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@contact_relationship_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Create_Contact_Relationship] 
	@contact_email varchar(254),
	@related_contact_email varchar(254),
	@relationship_id int,
	@start_date datetime,
	@error_message nvarchar(500) OUTPUT,
	@contact_relationship_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Required fields
	IF @contact_email is null
	BEGIN
		SET @error_message = 'Contact email cannot be null'+CHAR(13);
		RETURN;
	END;
	DECLARE @contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @contact_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@contact_email+CHAR(13);
		RETURN;
	END;

	IF @related_contact_email is null
	BEGIN
		SET @error_message = 'Related contact emails cannot be null'+CHAR(13);
		RETURN;
	END;
	DECLARE @related_contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @related_contact_email);
	IF @contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@related_contact_email+CHAR(13);
		RETURN;
	END;

	IF @related_contact_id is null
	BEGIN
		SET @error_message = 'Relationship id cannot be null'+CHAR(13);
		RETURN;
	END;

	--Create/Update relationship
	SET @contact_relationship_id = (SELECT Contact_Relationship_ID FROM [dbo].Contact_Relationships 
		WHERE Contact_ID = @contact_id and Related_Contact_ID = @related_contact_id and Relationship_ID = @relationship_id);
	IF @contact_relationship_id is null
	BEGIN
		--Reciprocal relationship will be created automatically
		INSERT INTO [dbo].Contact_Relationships
		(Contact_ID ,Relationship_ID ,Related_Contact_ID ,Domain_ID) VALUES
		(@contact_id,@relationship_id,@related_contact_id,1        );

		SET @contact_relationship_id = SCOPE_IDENTITY();
	END;

	IF @contact_relationship_id is not null
	BEGIN
		UPDATE [dbo].Contact_Relationships
		SET Start_Date = @start_date
		WHERE Contact_Relationship_ID = @contact_relationship_id;
	END;
END
GO