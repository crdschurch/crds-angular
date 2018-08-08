USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting participant data
-- =============================================

-- Defines cr_QA_Delete_Event_Participant
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Event_Participant')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Event_Participant
	@event_participant_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Event_Participant] 
	@event_participant_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @event_participant_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Event_Participant_Waivers WHERE Event_Participant_ID = @event_participant_id;
	DELETE [dbo].cr_EventParticipant_Documents WHERE Event_Participant_ID = @event_participant_id;
	
	--Nullify foreign keys
	UPDATE [dbo].Form_Response_Answers SET Event_Participant_ID = null WHERE Event_Participant_ID = @event_participant_id;
	UPDATE [dbo].Invoice_Detail SET Event_Participant_ID = null WHERE Event_Participant_ID = @event_participant_id;

	DELETE [dbo].Event_Participants WHERE Event_Participant_ID = @event_participant_id;
END
GO


-- Defines cr_QA_Delete_Group_Participant
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Group_Participant')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Group_Participant
	@group_participant_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Group_Participant] 
	@group_participant_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @group_participant_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Group_Participant_Attributes WHERE Group_Participant_ID = @group_participant_id;
	
	--Nullify foreign keys
	UPDATE [dbo].Event_Participants SET Group_Participant_ID = null WHERE Group_Participant_ID = @group_participant_id;

	DELETE [dbo].Group_Participants WHERE Group_Participant_ID = @group_participant_id;
END
GO


-- Defines cr_QA_Delete_Participant
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Participant')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Participant
	@participant_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Participant]
	@participant_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	if @participant_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Check_In_Log WHERE Participant_ID = @participant_id;
	DELETE [dbo].cr_Connect_History WHERE Participant_ID = @participant_id;
	DELETE [dbo].cr_Registrations WHERE Participant_ID = @participant_id;
	DELETE [dbo].Participant_Milestones WHERE Participant_ID = @participant_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Event Participants
	DECLARE @event_participants_to_delete TABLE
	(
		event_participant_id int
	)
	INSERT INTO @event_participants_to_delete (event_participant_id) SELECT Event_Participant_ID 
		FROM [dbo].Event_Participants WHERE Participant_ID = @participant_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 event_participant_id 
			FROM @event_participants_to_delete
			WHERE event_participant_id > @cur_entry_id
			ORDER BY event_participant_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Event_Participant] @cur_entry_id;
		END
	END

	--Delete Group Participants
	DECLARE @group_participants_to_delete TABLE
	(
		group_participant_id int
	)
	INSERT INTO @group_participants_to_delete (group_participant_id) SELECT Group_Participant_ID 
		FROM [dbo].Group_Participants WHERE Participant_ID = @participant_id;

	SET @cur_entry_id = 0;

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

	--Delete Responses
	DECLARE @responses_to_delete TABLE
	(
		response_id int
	)
	INSERT INTO @responses_to_delete (response_id) SELECT Response_ID 
		FROM [dbo].Responses WHERE Participant_ID = @participant_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 response_id 
			FROM @responses_to_delete
			WHERE response_id > @cur_entry_id
			ORDER BY response_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Response] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Contacts SET Participant_Record = null WHERE Participant_Record = @participant_id;

	DELETE [dbo].Participants WHERE Participant_ID = @participant_id;
END
GO