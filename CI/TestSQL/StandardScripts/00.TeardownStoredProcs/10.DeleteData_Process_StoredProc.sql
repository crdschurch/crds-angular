USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting process data.
-- =============================================

-- Defines cr_QA_Delete_Process_Submission
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Process_Submission')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Process_Submission @process_submission_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Process_Submission] 
	@process_submission_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @process_submission_id is null
		RETURN;

	--Nullify foreign keys
	UPDATE [dbo].dp_Tasks SET _Process_Submission_ID = null WHERE _Process_Submission_ID = @process_submission_id;
	
	DELETE [dbo].dp_Process_Submissions WHERE Process_Submission_ID = @process_submission_id;
END
GO


-- Defines cr_QA_Delete_Process
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Process')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Process
	@process_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Process]
	@process_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @process_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].dp_Process_Steps WHERE Process_ID = @process_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Process Submissions
	DECLARE @process_submissions_to_delete TABLE
	(
		process_submission_id int
	)
	INSERT INTO @process_submissions_to_delete (process_submission_id) SELECT Process_Submission_ID
		FROM [dbo].dp_Process_Submissions WHERE Process_ID = @process_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 process_submission_id 
			FROM @process_submissions_to_delete
			WHERE process_submission_id > @cur_entry_id
			ORDER BY process_submission_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Process_Submission] @cur_entry_id;
		END
	END
		
	DELETE [dbo].dp_Processes WHERE Process_ID = @process_id;
END
GO