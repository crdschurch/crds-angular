USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting congregation data.
-- =============================================


-- Defines cr_QA_Delete_Preferred_Serve_Time
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Preferred_Serve_Time')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Preferred_Serve_Time
	@preferred_serve_time_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Preferred_Serve_Time] 
	@preferred_serve_time_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @preferred_serve_time_id is null
		RETURN;

	--Nullify foreign keys
	UPDATE [dbo].Group_Participants SET Preferred_Serving_Time_ID = null WHERE Preferred_Serving_Time_ID = @preferred_serve_time_id;
	
	DELETE [dbo].cr_Preferred_Serve_Time WHERE Preferred_Serving_Time_ID = @preferred_serve_time_id;
END
GO


-- Defines cr_QA_Delete_Congregation
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Congregation')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Congregation
	@congregation_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Congregation]
	@congregation_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @congregation_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Childcare_Preferred_Times WHERE Congregation_ID = @congregation_id;
	DELETE [dbo].cr_Kiosk_Configs WHERE Congregation_ID = @congregation_id;
	DELETE [dbo].GL_Account_Mapping WHERE Congregation_ID = @congregation_id;
	DELETE [dbo].Payment_Detail WHERE Congregation_ID = @congregation_id;
	DELETE [dbo].User_Congregations WHERE Congregation_ID = @congregation_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Childcare Requests
	DECLARE @childcare_requests_to_delete TABLE
	(
		childcare_request_id int
	)
	INSERT INTO @childcare_requests_to_delete (childcare_request_id) SELECT Childcare_Request_ID 
		FROM [dbo].cr_Childcare_Requests WHERE Congregation_ID = @congregation_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 childcare_request_id 
			FROM @childcare_requests_to_delete
			WHERE childcare_request_id > @cur_entry_id
			ORDER BY childcare_request_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Childcare_Request] @cur_entry_id;
		END
	END

	--Delete Preferred Serve Times
	DECLARE @preferred_serve_times_to_delete TABLE
	(
		preferred_serve_time_id int
	)
	INSERT INTO @preferred_serve_times_to_delete (preferred_serve_time_id) SELECT Preferred_Serving_Time_ID 
		FROM [dbo].cr_Preferred_Serve_Time WHERE Congregation_ID = @congregation_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 preferred_serve_time_id 
			FROM @preferred_serve_times_to_delete
			WHERE preferred_serve_time_id > @cur_entry_id
			ORDER BY preferred_serve_time_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Preferred_Serve_Time] @cur_entry_id;
		END
	END

	--Delete Events
	DECLARE @events_to_delete TABLE
	(
		event_id int
	)
	INSERT INTO @events_to_delete (event_id) SELECT Event_ID 
		FROM [dbo].Events WHERE Congregation_ID = @congregation_id;

	SET @cur_entry_id = 0;

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

	--Delete Groups
	DECLARE @groups_to_delete TABLE
	(
		group_id int
	)
	INSERT INTO @groups_to_delete (group_id) SELECT Group_ID 
		FROM [dbo].Groups WHERE Congregation_ID = @congregation_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 group_id 
			FROM @groups_to_delete
			WHERE group_id > @cur_entry_id
			ORDER BY group_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Group] @cur_entry_id, null;
		END
	END

	--Delete Programs
	DECLARE @programs_to_delete TABLE
	(
		program_id int
	)
	INSERT INTO @programs_to_delete (program_id) SELECT Program_ID 
		FROM [dbo].Programs WHERE Congregation_ID = @congregation_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 program_id 
			FROM @programs_to_delete
			WHERE program_id > @cur_entry_id
			ORDER BY program_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Program] @cur_entry_id, null;
		END
	END
	
	--Delete Recurring Gifts
	DECLARE @recurring_gifts_to_delete TABLE
	(
		recurring_gift_id int
	)
	INSERT INTO @recurring_gifts_to_delete (recurring_gift_id) SELECT Recurring_Gift_ID 
		FROM [dbo].Recurring_Gifts WHERE Congregation_ID = @congregation_id;

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
	UPDATE [dbo].Activity_Log SET Congregation_ID = null WHERE Congregation_ID = @congregation_id;
	UPDATE [dbo].Batches SET Congregation_ID = null WHERE Congregation_ID = @congregation_id;
	UPDATE [dbo].Contact_Staging SET Congregation_ID = null WHERE Congregation_ID = @congregation_id;
	UPDATE [dbo].Donation_Distributions SET Congregation_ID = null WHERE Congregation_ID = @congregation_id;
	UPDATE [dbo].Donation_Distributions SET HC_Donor_Congregation_ID = null WHERE HC_Donor_Congregation_ID = @congregation_id;
	UPDATE [dbo].Households SET Congregation_ID = null WHERE Congregation_ID = @congregation_id;

	DELETE [dbo].Congregations WHERE Congregation_ID = @congregation_id;
END
GO