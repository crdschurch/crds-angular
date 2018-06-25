USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting group data
-- =============================================

-- Defines cr_QA_Delete_Group
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Group')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Group
	@group_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Group]
	@group_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @group_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Event_Groups WHERE Group_ID = @group_id;
	DELETE [dbo].Group_Assessments WHERE Group_ID = @group_id;
	DELETE [dbo].Group_Attributes WHERE Group_ID = @group_id;
	DELETE [dbo].Group_Inquiries WHERE Group_ID = @group_id;
	DELETE [dbo].Group_Rooms WHERE Group_ID = @group_id;
	DELETE [dbo].Program_Groups WHERE Group_ID = @group_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Group Participants
	DECLARE @group_participants_to_delete TABLE
	(
		group_participant_id int
	)
	INSERT INTO @group_participants_to_delete (group_participant_id) SELECT Group_Participant_ID 
		FROM [dbo].Group_Participants WHERE Group_ID = @group_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 group_participant_id 
			FROM @group_participants_to_delete
			WHERE group_participant_id > @cur_entry_id
			ORDER BY group_participant_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Group_Participant] @cur_entry_id;
		END
	END

	--Delete Childcare Requests
	DECLARE @childcare_requests_to_delete TABLE
	(
		childcare_request_id int
	)
	INSERT INTO @childcare_requests_to_delete (childcare_request_id) SELECT Childcare_Request_ID 
		FROM [dbo].cr_Childcare_Requests WHERE Group_ID = @group_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 childcare_request_id 
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
	UPDATE [dbo].cr_Connect_Communications SET Group_ID = null WHERE Group_ID = @group_id;
	UPDATE [dbo].Event_Metrics SET Group_ID = null WHERE Group_ID = @group_id;
	UPDATE [dbo].Event_Participants SET Group_ID = null WHERE Group_ID = @group_id;
	UPDATE [dbo].Feedback_Entries SET Group_ID = null WHERE Group_ID = @group_id;
	UPDATE [dbo].File_Library SET Group_ID = null WHERE Group_ID = @group_id;
	UPDATE [dbo].Groups SET Parent_Group = null WHERE Parent_Group = @group_id;
	UPDATE [dbo].Groups SET Descended_From = null WHERE Descended_From = @group_id;
	UPDATE [dbo].Groups SET Promote_to_Group = null WHERE Promote_to_Group = @group_id;
	UPDATE [dbo].Journeys SET Leadership_Team = null WHERE Leadership_Team = @group_id;
	UPDATE [dbo].Ministries SET Leadership_Team = null WHERE Leadership_Team = @group_id;
	UPDATE [dbo].Opportunities SET Add_to_Group = null WHERE Add_to_Group = @group_id;
	UPDATE [dbo].Planned_Contacts SET Call_Team = null WHERE Call_Team = @group_id;
	UPDATE [dbo].Product_Option_Prices SET Add_to_Group = null WHERE Add_to_Group = @group_id;
	UPDATE [dbo].Programs SET Leadership_Team = null WHERE Leadership_Team = @group_id;
	UPDATE [dbo].Servicing SET Team_Group_ID = null WHERE Team_Group_ID = @group_id;
	
	DELETE [dbo].Groups WHERE Group_ID = @group_id;
END
GO


-- Defines cr_QA_Delete_Group_By_Name
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Group_By_Name')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Group_By_Name
	@group_name nvarchar(75) AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Group_By_Name]
	@group_name nvarchar(75)
AS
BEGIN
	SET NOCOUNT ON;

	IF @group_name is null
		RETURN;

	--Delete Groups by name
	DECLARE @groups_to_delete TABLE
	(
		group_id int
	)
	INSERT INTO @groups_to_delete (group_id) SELECT Group_ID FROM [dbo].Groups WHERE Group_Name = @group_name;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 group_id 
			FROM @groups_to_delete
			WHERE group_id > @cur_entry_id
			ORDER BY group_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Group] @cur_entry_id;
		END
	END
END
GO