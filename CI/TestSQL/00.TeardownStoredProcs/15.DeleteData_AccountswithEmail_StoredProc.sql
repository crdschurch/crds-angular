USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/10/2018
-- Description:	Deletes contacts and users with a given email address (including guest givers) and all data related to those accounts.
-- =============================================

-- Defines cr_QA_Delete_Accounts_With_Email
IF NOT EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Accounts_With_Email')
                    AND type IN ( N'P', N'PC' ) )
            EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Accounts_With_Email
			@contact_email varchar(255) AS SET NOCOUNT ON;')			
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Accounts_With_Email]
	@contact_email varchar(255)
AS
BEGIN
	SET NOCOUNT ON;

	IF @contact_email is null
		RETURN;

	--Delete contacts with email
	DECLARE @contacts_to_delete TABLE
	(
		contact_id int
	)
	INSERT INTO @contacts_to_delete (contact_id) SELECT Contact_ID FROM [dbo].Contacts
		WHERE Email_Address = @contact_email;
				
	--Delete contacts
	DECLARE @cur_contact_id int = 0;
	WHILE @cur_contact_id is not null
	BEGIN
		--Get top contact in deletion list
		SET @cur_contact_id = (SELECT TOP 1 contact_id 
			FROM @contacts_to_delete
			WHERE contact_id > @cur_contact_id
			ORDER BY contact_id ASC);

		--Delete contact and related data
		IF @cur_contact_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Contact] @cur_contact_id;
		END
	END
		
	--Delete users with email address
	--This will catch users who were attached to a contact with the wrong email and not correctly resynced with their contact
	DECLARE @users_to_delete TABLE
	(
		user_id int
	)
	INSERT INTO @users_to_delete (user_id) SELECT User_ID FROM [dbo].dp_Users
		WHERE User_Name = @contact_email OR User_Email = @contact_email;

	--Delete users
	DECLARE @cur_user_id int = 0;
	WHILE @cur_user_id is not null
	BEGIN
		--Get top user in deletion list
		SET @cur_user_id = (SELECT TOP 1 user_id 
			FROM @users_to_delete
			WHERE user_id > @cur_user_id
			ORDER BY user_id ASC);

		--Delete user
		IF @cur_user_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_User] @cur_user_id;
		END
	END
END
GO