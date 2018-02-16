USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting program data.
-- =============================================

-- Defines cr_QA_Delete_Program
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Program')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Program
	@program_id int,
	@program_name nvarchar(130) AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Program]
	@program_id int,
	@program_name nvarchar(130)
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @program_id is null and @program_name is not null
		SET @program_id = (SELECT TOP 1 Program_ID FROM [dbo].Programs WHERE Program_Name = @program_name ORDER BY Program_ID ASC);

	IF @program_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Initiatives WHERE Program_ID = @program_id;
	DELETE [dbo].Donation_Distributions WHERE Program_ID = @program_id;
	DELETE [dbo].Participant_Milestones WHERE Program_ID = @program_id;
	DELETE [dbo].Program_Groups WHERE Program_ID = @program_id;
	DELETE [dbo].Scheduled_Donations WHERE Program_ID = @program_id;
	DELETE [dbo].GL_Account_Mapping WHERE Program_ID = @program_id;
	
	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Events
	DECLARE @events_to_delete TABLE
	(
		event_id int
	)
	INSERT INTO @events_to_delete (event_id) SELECT Event_ID 
		FROM [dbo].Events WHERE Program_ID = @program_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 event_id 
			FROM @events_to_delete
			WHERE event_id > @cur_entry_id
			ORDER BY event_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Event] @cur_entry_id, null, null;
		END
	END

	--Delete Opportunities
	DECLARE @oppurtunities_to_delete TABLE
	(
		oppurtunity_id int
	)
	INSERT INTO @oppurtunities_to_delete (oppurtunity_id) SELECT Opportunity_ID 
		FROM [dbo].Opportunities WHERE Program_ID = @program_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 oppurtunity_id 
			FROM @oppurtunities_to_delete
			WHERE oppurtunity_id > @cur_entry_id
			ORDER BY oppurtunity_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Opportunity] @cur_entry_id, null;
		END
	END

	--Delete Recurring Gifts
	DECLARE @recurring_gifts_to_delete TABLE
	(
		recurring_gift_id int
	)
	INSERT INTO @recurring_gifts_to_delete (recurring_gift_id) SELECT Recurring_Gift_ID 
		FROM [dbo].Recurring_Gifts WHERE Program_ID = @program_id;

	SET @cur_entry_id = 0;

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
	UPDATE [dbo].Batches SET Default_Program = null WHERE Default_Program = @program_id;
	UPDATE [dbo].Feedback_Entries SET Program_ID = null WHERE Program_ID = @program_id;
	UPDATE [dbo].Pledge_Campaigns SET Program_ID = null WHERE Program_ID = @program_id;
	UPDATE [dbo].Products SET Program_ID = null WHERE Program_ID = @program_id;

	DELETE [dbo].Programs WHERE Program_ID = @program_id;
END
GO