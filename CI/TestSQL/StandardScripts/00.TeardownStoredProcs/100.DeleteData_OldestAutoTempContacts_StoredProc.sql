USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date:	02/10/2020
-- Description:	Stored procedure declaration for deleting oldest x temp test users
-- =============================================

-- Defines cr_QA_Delete_Temp_Auto_Users
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Temp_Auto_Users')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Temp_Auto_Users
	@count_to_delete int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Temp_Auto_Users] 
	@count_to_delete int
AS
BEGIN
	SET NOCOUNT ON;

	--This should take between 5-6 sec to delete a contact fully
	DECLARE @auto_temp_contacts TABLE (contact_id int);

	--SELECT only one person per household so we can accurately delete newly empty households
	INSERT INTO @auto_temp_contacts (contact_id)
	(
	SELECT TOP (@count_to_delete)
	MIN(Contact_ID) as "Contact_ID"
	FROM [dbo].Contacts
	WHERE Email_Address LIKE 'mpcrds+auto+temp+%'
	GROUP BY Household_ID
	)
	ORDER BY Contact_ID ASC;

	--Delete contacts
	DECLARE @cur_contact_id int = 0;
	WHILE @cur_contact_id is not null
	BEGIN
		--Get top contact in deletion list
		SET @cur_contact_id = (SELECT TOP 1 contact_id 
			FROM @auto_temp_contacts
			WHERE contact_id > @cur_contact_id
			ORDER BY contact_id ASC);

		--Delete contact and related data
		IF @cur_contact_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Contact] @cur_contact_id;
		END
	END
END
GO