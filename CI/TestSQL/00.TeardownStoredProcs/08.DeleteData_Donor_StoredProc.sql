USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting donor data.
-- =============================================

-- Defines cr_QA_Delete_Donation
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Donation')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Donation @donation_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Donation] 
	@donation_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @donation_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Donation_Communications WHERE Donation_ID = @donation_id;
	DELETE [dbo].Donation_Distributions WHERE Donation_ID = @donation_id;

	DELETE [dbo].Donations WHERE Donation_ID = @donation_id;
END
GO


-- Defines cr_QA_Delete_Donor
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Donor')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Donor @donor_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Donor]
	@donor_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @donor_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Scheduled_Donations WHERE Donor_ID = @donor_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Responses
	DECLARE @recurring_gifts_to_delete TABLE
	(
		recurring_gift_id int
	)
	INSERT INTO @recurring_gifts_to_delete (recurring_gift_id) SELECT Recurring_Gift_ID 
		FROM [dbo].Recurring_Gifts WHERE Donor_ID = @donor_id;

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

	--Delete Pledges
	DECLARE @pledges_to_delete TABLE
	(
		pledge_id int
	)
	INSERT INTO @pledges_to_delete (pledge_id) SELECT Pledge_ID 
		FROM [dbo].Pledges WHERE Donor_ID = @donor_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 pledge_id 
			FROM @pledges_to_delete
			WHERE pledge_id > @cur_entry_id
			ORDER BY pledge_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Pledge] @cur_entry_id;
		END
	END

	--Delete Donor Accounts
	DECLARE @donor_accounts_to_delete TABLE
	(
		donor_account_id int
	)
	INSERT INTO @donor_accounts_to_delete (donor_account_id) SELECT Donor_Account_ID 
		FROM [dbo].Donor_Accounts WHERE Donor_ID = @donor_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 donor_account_id 
			FROM @donor_accounts_to_delete
			WHERE donor_account_id > @cur_entry_id
			ORDER BY donor_account_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Donor_Account] @cur_entry_id;
		END
	END
	
	--Delete Donations
	DECLARE @donations_to_delete TABLE
	(
		donation_id int
	)
	INSERT INTO @donations_to_delete (donation_id) SELECT Donation_ID 
		FROM [dbo].Donations WHERE Donor_ID = @donor_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 donation_id 
			FROM @donations_to_delete
			WHERE donation_id > @cur_entry_id
			ORDER BY donation_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Donation] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Contacts SET Donor_Record = null WHERE Donor_Record = @donor_id;
	UPDATE [dbo].Donation_Distributions SET Soft_Credit_Donor = null WHERE Soft_Credit_Donor = @donor_id;

	DELETE [dbo].Donors WHERE Donor_ID = @donor_id;
END
GO