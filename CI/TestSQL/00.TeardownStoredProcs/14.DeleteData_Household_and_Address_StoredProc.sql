USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/05/2018
-- Description:	Stored procedure declarations for deleting Household and Address data
-- =============================================

-- Defines cr_QA_Delete_Address
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Address')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Address
	@address_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Address] 
	@address_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @address_id is null
		RETURN;

	--Nullify foreign keys
	UPDATE [dbo].Households SET Address_ID = null WHERE Address_ID = @address_id;
	UPDATE [dbo].Households SET Alternate_Mailing_Address = null WHERE Alternate_Mailing_Address = @address_id;
	UPDATE [dbo].Groups SET Offsite_Meeting_Address = null WHERE Offsite_Meeting_Address = @address_id;
	UPDATE [dbo].cr_Projects SET Address_ID = null WHERE Address_ID = @address_id;

	DELETE [dbo].Addresses WHERE Address_ID = @address_id;
END
GO


-- Defines cr_QA_Delete_Household
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Household')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Household
	@household_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Household] 
	@household_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @household_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Household_Care_Log WHERE Household_ID = @household_id;
	DELETE [dbo].Contact_Households WHERE Household_ID = @household_id;
	DELETE [dbo].Care_Cases WHERE Household_ID = @household_id;

	--Delete Address (deletion not required, but should be for cleaner data)
	DECLARE @addresses_to_delete TABLE
	(
		address_id int
	)
	INSERT INTO @addresses_to_delete (address_id) SELECT Address_ID 
		FROM [dbo].Households WHERE Household_ID = @household_id AND Address_ID is not null;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 address_id 
			FROM @addresses_to_delete
			WHERE address_id > @cur_entry_id
			ORDER BY address_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Address] @cur_entry_id;
		END
	END
	
	--Nullify foreign keys
	UPDATE [dbo].Contacts SET Household_ID = null WHERE Household_ID = @household_id;
	UPDATE [dbo].Contact_Staging SET Existing_Household_Record = null WHERE Existing_Household_Record = @household_id;
	UPDATE [dbo].Activity_Log SET Household_ID = null WHERE Household_ID = @household_id;

	DELETE [dbo].Households WHERE Household_ID = @household_id;
END
GO