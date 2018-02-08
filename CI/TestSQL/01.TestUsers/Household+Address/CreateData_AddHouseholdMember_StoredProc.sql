USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/16/2018
-- Description:	Add contact to household of another contact
-- =============================================


-- Defines cr_QA_Add_Household_Member
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Add_Household_Member')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Add_Household_Member
	@household_member_email varchar(254),
	@new_member_email varchar(254),
	@error_message nvarchar(500) OUTPUT,
	@household_id int OUTPUT AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Add_Household_Member] 
	@household_member_email varchar(254),
	@new_member_email varchar(254),
	@error_message nvarchar(500) OUTPUT,
	@household_id int OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	--Enforce required parameters
	IF @household_member_email is null OR @new_member_email is null
	BEGIN
		SET @error_message = 'Household member and new member emails cannot be null'+CHAR(13);
		RETURN;
	END;

	
	--Required fields
	DECLARE @household_contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @household_member_email);
	IF @household_contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@household_member_email+CHAR(13);
		RETURN;
	END;
	
	SET @household_id = (SELECT Household_ID FROM [dbo].Contacts WHERE Contact_ID = @household_contact_id);
	IF @household_id is null
	BEGIN
		--Use defaults
		EXEC [dbo].[cr_QA_Create_Household] @household_member_email, null, null,
		@error_message = @error_message OUTPUT, @household_id = @household_id OUTPUT;
		IF @household_id is null
		BEGIN
			SET @error_message = @error_message + 'Could not create household for '+@household_member_email+CHAR(13);
			RETURN;
		END;
	END;

	DECLARE @new_contact_id int = (SELECT Contact_ID FROM [dbo].dp_Users WHERE User_Name = @new_member_email);
	IF @new_contact_id is null
	BEGIN
		SET @error_message = 'Could not find contact with email '+@new_member_email+CHAR(13);
		RETURN;
	END;
	
	DECLARE @old_household_id int;
	SET @old_household_id = (SELECT Household_ID FROM [dbo].Contacts WHERE Contact_ID = @new_contact_id);


	--Add contact to new household
	UPDATE [dbo].Contacts SET Household_ID = @household_id WHERE Contact_ID = @new_contact_id;


	--Delete old household if empty now
	IF @old_household_id is not null
	BEGIN
		IF (SELECT count(Contact_ID) FROM [dbo].Contacts WHERE Household_ID = @old_household_id) = 0
			EXEC [dbo].[cr_QA_Delete_Household] @old_household_id;
	END
END
GO