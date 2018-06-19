USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting donor account data.
-- =============================================

-- Defines cr_QA_Delete_Donor_Account
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Donor_Account')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Donor_Account
	@donor_account_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Donor_Account]
	@donor_account_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @donor_account_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Scheduled_Donations WHERE Donor_Account_ID = @donor_account_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Responses
	DECLARE @recurring_gifts_to_delete TABLE
	(
		recurring_gift_id int
	)
	INSERT INTO @recurring_gifts_to_delete (recurring_gift_id) SELECT Recurring_Gift_ID 
		FROM [dbo].Recurring_Gifts WHERE Donor_Account_ID = @donor_account_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 recurring_gift_id 
			FROM @recurring_gifts_to_delete
			WHERE recurring_gift_id > @cur_entry_id
			ORDER BY recurring_gift_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Recurring_Gift] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Donations SET Donor_Account_ID = null WHERE Donor_Account_ID = @donor_account_id;
	
	DELETE [dbo].Donor_Accounts WHERE Donor_Account_ID = @donor_account_id;
END
GO