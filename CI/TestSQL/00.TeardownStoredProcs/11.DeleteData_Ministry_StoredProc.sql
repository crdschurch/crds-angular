USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting ministry data.
-- =============================================

-- Defines cr_QA_Delete_Ministry
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Ministry')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Ministry
	@ministry_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Ministry]
	@ministry_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @ministry_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Serve_Restrictions WHERE Ministry_ID = @ministry_id;
	DELETE [dbo].Ministry_Updates WHERE Ministry_ID = @ministry_id;
	DELETE [dbo].Purchase_Requests WHERE Ministry_ID = @ministry_id;
	DELETE [dbo].User_Ministries WHERE Ministry_ID = @ministry_id;
	
	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Programs
	DECLARE @programs_to_delete TABLE
	(
		program_id int
	)
	INSERT INTO @programs_to_delete (program_id) SELECT Program_ID 
		FROM [dbo].Programs WHERE Ministry_ID = @ministry_id;

	DECLARE @cur_entry_id int = 0;

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

	--Delete Groups
	DECLARE @groups_to_delete TABLE
	(
		group_id int
	)
	INSERT INTO @groups_to_delete (group_id) SELECT Group_ID 
		FROM [dbo].Groups WHERE Ministry_ID = @ministry_id;

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

	--Delete Childcare Requests
	DECLARE @childcare_requests_to_delete TABLE
	(
		childcare_request_id int
	)
	INSERT INTO @childcare_requests_to_delete (childcare_request_id) SELECT Childcare_Request_ID 
		FROM [dbo].cr_Childcare_Requests WHERE Ministry_ID = @ministry_id;

	SET @cur_entry_id = 0;

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

	--Nullify foreign keys
	UPDATE [dbo].Activity_Log SET Ministry_ID = null WHERE Ministry_ID = @ministry_id;
	UPDATE [dbo].Background_Checks SET Requesting_Ministry = null WHERE Requesting_Ministry = @ministry_id;
	UPDATE [dbo].Group_Roles SET Ministry_ID = null WHERE Ministry_ID = @ministry_id;
	UPDATE [dbo].Procedures SET Ministry_ID = null WHERE Ministry_ID = @ministry_id;

	DELETE [dbo].Ministries WHERE Ministry_ID = @ministry_id;
END
GO