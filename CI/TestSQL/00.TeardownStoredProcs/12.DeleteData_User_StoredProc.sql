USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/09/2018
-- Description:	Stored procedure declarations for deleting user data
-- =============================================


-- Defines cr_QA_Delete_Notification
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Notification')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Notification
	@notification_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Notification] 
	@notification_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @notification_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].dp_Notification_Page_Views WHERE Notification_ID = @notification_id;
	DELETE [dbo].dp_Notification_Sub_Page_Views WHERE Notification_ID = @notification_id;
	
	DELETE [dbo].dp_Notifications WHERE Notification_ID = @notification_id;
END
GO


-- Defines cr_QA_Delete_Selection
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Selection')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Selection
	@selection_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Selection] 
	@selection_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @selection_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].dp_Selected_Records WHERE Selection_ID = @selection_id;

	--Nullify foreign keys
	UPDATE [dbo].dp_Communications SET Selection_ID = null WHERE Selection_ID = @selection_id;
		
	DELETE [dbo].dp_Selections WHERE Selection_ID = @selection_id;
END
GO


-- Defines cr_QA_Delete_Task
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Task')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Task
	@task_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Task] 
	@task_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @task_id is null
		RETURN;

	--Nullify foreign keys
	UPDATE [dbo].dp_Commands SET Task_ID = null WHERE Task_ID = @task_id;
	
	DELETE [dbo].dp_Tasks WHERE Task_ID = @task_id;
END
GO


-- Defines cr_QA_Delete_User
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_User')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_User @user_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_User]
	@user_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @user_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Contact_Log WHERE Made_By = @user_id;
	DELETE [dbo].dp_API_Clients WHERE Client_User_ID = @user_id;
	DELETE [dbo].dp_Impersonate_Contacts WHERE User_ID = @user_id;
	DELETE [dbo].dp_Record_Users WHERE User_ID = @user_id;
	DELETE [dbo].dp_User_Identities WHERE User_ID = @user_id;
	DELETE [dbo].dp_User_Publications WHERE User_ID = @user_id;
	DELETE [dbo].dp_User_Roles WHERE User_ID = @user_id;
	DELETE [dbo].dp_User_User_Groups WHERE User_ID = @user_id;
	DELETE [dbo].IT_Help_Tickets WHERE Submitted_For = @user_id;
	DELETE [dbo].Maintenance_Requests WHERE Submitted_For = @user_id;
	DELETE [dbo].Ministry_Updates WHERE Submitted_By = @user_id;
	DELETE [dbo].Planned_Contacts WHERE Manager = @user_id;
	DELETE [dbo].Procedures WHERE User_ID = @user_id;
	DELETE [dbo].Purchase_Requests WHERE Submitted_By = @user_id;
	DELETE [dbo].Suggestions WHERE Suggested_By = @user_id;
	DELETE [dbo].Time_Off WHERE Staff_Member = @user_id;
	DELETE [dbo].User_Congregations WHERE User_ID = @user_id;
	DELETE [dbo].User_Ministries WHERE User_ID = @user_id;

	--Delete foreign key entries that can't be nullified using another stored proc	
	--Delete Congregations
	DECLARE @congregations_to_delete TABLE
	(
		congregation_id int
	)
	INSERT INTO @congregations_to_delete (congregation_id) SELECT Congregation_ID
		FROM [dbo].Congregations WHERE Pastor = @user_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 congregation_id 
			FROM @congregations_to_delete
			WHERE congregation_id > @cur_entry_id
			ORDER BY congregation_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Congregation] @cur_entry_id;
		END
	END

	--Delete Communications
	DECLARE @communications_to_delete TABLE
	(
		communication_id int
	)
	INSERT INTO @communications_to_delete (communication_id) SELECT Communication_ID 
		FROM [dbo].dp_Communications WHERE Author_User_ID = @user_id;
		
	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 communication_id 
			FROM @communications_to_delete
			WHERE communication_id > @cur_entry_id
			ORDER BY communication_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Communication] @cur_entry_id;
		END
	END

	--Delete Notification
	DECLARE @notifications_to_delete TABLE
	(
		notification_id int
	)
	INSERT INTO @notifications_to_delete (notification_id) SELECT Notification_ID 
		FROM [dbo].dp_Notifications WHERE User_ID = @user_id;
		
	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 notification_id 
			FROM @notifications_to_delete
			WHERE notification_id > @cur_entry_id
			ORDER BY notification_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Notification] @cur_entry_id;
		END
	END

	--Delete Process Submissions
	DECLARE @process_submissions_to_delete TABLE
	(
		process_submission_id int
	)
	INSERT INTO @process_submissions_to_delete (process_submission_id) SELECT Process_Submission_ID
		FROM [dbo].dp_Process_Submissions WHERE Submitted_By = @user_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 process_submission_id 
			FROM @process_submissions_to_delete
			WHERE process_submission_id > @cur_entry_id
			ORDER BY process_submission_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Process_Submission] @cur_entry_id;
		END
	END
	
	--Delete Processes
	DECLARE @processes_to_delete TABLE
	(
		process_id int
	)
	INSERT INTO @processes_to_delete (process_id) SELECT Process_ID
		FROM [dbo].dp_Processes WHERE Process_Manager = @user_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 process_id 
			FROM @processes_to_delete
			WHERE process_id > @cur_entry_id
			ORDER BY process_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Process] @cur_entry_id;
		END
	END

	--Delete Selections
	DECLARE @selections_to_delete TABLE
	(
		selection_id int
	)
	INSERT INTO @selections_to_delete (selection_id) SELECT Selection_ID
		FROM [dbo].dp_Selections WHERE User_ID = @user_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 selection_id 
			FROM @selections_to_delete
			WHERE selection_id > @cur_entry_id
			ORDER BY selection_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Selection] @cur_entry_id;
		END
	END

	--Delete Tasks
	DECLARE @tasks_to_delete TABLE
	(
		task_id int
	)
	INSERT INTO @tasks_to_delete (task_id) SELECT Task_ID
		FROM [dbo].dp_Tasks WHERE Assigned_User_ID = @user_id;
	INSERT INTO @tasks_to_delete (task_id) SELECT Task_ID
		FROM [dbo].dp_Tasks WHERE Author_User_ID = @user_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		SET @cur_entry_id = (SELECT TOP 1 task_id 
			FROM @tasks_to_delete
			WHERE task_id > @cur_entry_id
			ORDER BY task_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Task] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Batches SET Operator_User = null WHERE Operator_User = @user_id;
	UPDATE [dbo].Care_Cases SET Case_Manager = null WHERE Case_Manager = @user_id;
	UPDATE [dbo].Care_Types SET User_ID = null WHERE User_ID = @user_id;
	UPDATE [dbo].Contacts SET User_Account = null WHERE User_Account = @user_id;
	UPDATE [dbo].dp_Domains SET API_Service_Anonymous_User_Id = null WHERE API_Service_Anonymous_User_Id = @user_id;
	UPDATE [dbo].dp_Page_Section_Pages SET User_ID = null WHERE User_ID = @user_id;
	UPDATE [dbo].dp_Page_Views SET User_ID = null WHERE User_ID = @user_id;
	UPDATE [dbo].dp_Process_Steps SET Specific_User = null WHERE Specific_User = @user_id;
	UPDATE [dbo].dp_Publications SET Moderator = null WHERE Moderator = @user_id;
	UPDATE [dbo].dp_Sub_Page_Views SET User_ID = null WHERE User_ID = @user_id;
	UPDATE [dbo].dp_User_Groups SET Moderator = null WHERE Moderator = @user_id;
	UPDATE [dbo].Equipment SET Equipment_Coordinator = null WHERE Equipment_Coordinator = @user_id;
	UPDATE [dbo].Migration_Table_Notes SET Assigned_To = null WHERE Assigned_To = @user_id;
	UPDATE [dbo].Migration_Tables SET Assigned_To = null WHERE Assigned_To = @user_id;
	UPDATE [dbo].Planned_Contacts SET Next_Contact_By = null WHERE Next_Contact_By = @user_id;

	DELETE [dbo].dp_Users WHERE User_ID = @user_id;
END
GO