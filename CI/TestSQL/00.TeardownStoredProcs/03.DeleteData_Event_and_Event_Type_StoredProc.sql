USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date: 01/08/2018
-- Description:	Stored procedure declarations for deleting event and event type data
-- =============================================

-- Defines cr_QA_Delete_Event_Room
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Event_Room')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Event_Room
	@event_room_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Event_Room] 
	@event_room_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @event_room_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Bumping_Rules WHERE (From_Event_Room_ID = @event_room_id) 
		OR (To_Event_Room_ID = @event_room_id);
		
	--Nullify foreign keys
	UPDATE [dbo].Event_Groups SET Event_Room_ID = null WHERE Event_Room_ID = @event_room_id;

	DELETE [dbo].Event_Rooms WHERE Event_Room_ID = @event_room_id;
END
GO


-- Defines cr_QA_Delete_Event
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Event')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Event
	@event_id int,
	@event_name nvarchar(75),
	@start_date datetime AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Event]
	@event_id int,
	@event_name nvarchar(75),
	@start_date datetime
AS
BEGIN
	SET NOCOUNT ON;

	IF @event_id is null AND @event_name is not null AND @start_date is not null
		SET @event_id = (SELECT TOP 1 Event_ID FROM [dbo].Events WHERE Event_Title = @event_name AND Event_Start_Date = @start_date ORDER BY Event_ID ASC);
	
	IF @event_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified
	DELETE [dbo].Agenda_Elements WHERE Event_ID = @event_id;
	DELETE [dbo].cr_Event_Waivers WHERE Event_ID = @event_id;
	DELETE [dbo].Event_Equipment WHERE Event_ID = @event_id;
	DELETE [dbo].Event_Groups WHERE Event_ID = @event_id;
	DELETE [dbo].Event_Metrics WHERE Event_ID = @event_id;
	DELETE [dbo].Event_Services WHERE Event_ID = @event_id;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Event Participants
	DECLARE @event_participants_to_delete TABLE
	(
		event_participant_id int
	)
	INSERT INTO @event_participants_to_delete (event_participant_id) SELECT Event_Participant_ID 
		FROM [dbo].Event_Participants WHERE Event_ID = @event_id;

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

	--Delete Event Rooms
	DECLARE @event_rooms_to_delete TABLE
	(
		event_room_id int
	)
	INSERT INTO @event_rooms_to_delete (event_room_id) SELECT Event_Room_ID 
		FROM [dbo].Event_Rooms WHERE Event_ID = @event_id;

	SET @cur_entry_id = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 event_room_id 
			FROM @event_rooms_to_delete
			WHERE event_room_id > @cur_entry_id
			ORDER BY event_room_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Event_Room] @cur_entry_id;
		END
	END

	--Delete child events
	DECLARE @child_events_to_delete TABLE
	(
		child_event_id int
	)
	INSERT INTO @child_events_to_delete (child_event_id) SELECT Event_ID
		FROM [dbo].Events WHERE Parent_Event_ID = @event_id;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 child_event_id 
			FROM @child_events_to_delete
			WHERE child_event_id > @cur_entry_id
			ORDER BY child_event_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Event] @cur_entry_id;
		END
	END

	--Nullify foreign keys
	UPDATE [dbo].Batches SET Source_Event = null WHERE Source_Event = @event_id;
	UPDATE [dbo].Donation_Distributions SET Target_Event = null WHERE Target_Event = @event_id;
	UPDATE [dbo].Form_Responses SET Event_ID = null WHERE Event_ID = @event_id;
	UPDATE [dbo].Opportunities SET Add_to_Event = null WHERE Add_to_Event = @event_id;
	UPDATE [dbo].Participant_Milestones SET Event_ID = null WHERE Event_ID = @event_id;
	UPDATE [dbo].Pledge_Campaigns SET Event_ID = null WHERE Event_ID = @event_id;
	UPDATE [dbo].Programs SET Default_Target_Event = null WHERE Default_Target_Event = @event_id;
	UPDATE [dbo].Responses SET Event_ID = null WHERE Event_ID = @event_id;
	UPDATE [dbo].Scheduled_Donations SET Target_Event = null WHERE Target_Event = @event_id;

	DELETE [dbo].Events WHERE Event_ID = @event_id;
END
GO


-- Defines cr_QA_Delete_Event_Type
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Event_Type')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Event_Type
	@event_type_id int,
	@event_type_name nvarchar(50) AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Event_Type] 
	@event_type_id int,
	@event_type_name nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF @event_type_id is null AND @event_type_name is not null
		SET @event_type_id = (SELECT TOP 1 Event_Type_ID FROM [dbo].Event_Types WHERE Event_Type = @event_type_name ORDER BY Event_Type_ID ASC);

	IF @event_type_id is null
		RETURN;

	--Delete foreign key entries that can't be nullified using another stored proc
	--Delete Events
	DECLARE @events_to_delete TABLE
	(
		event_id int
	)
	INSERT INTO @events_to_delete (event_id) SELECT Event_ID 
		FROM [dbo].Events WHERE Event_Type_ID = @event_type_id;

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

	--Nullify foreign keys
	UPDATE [dbo].Opportunities SET Event_Type_ID = null WHERE Event_Type_ID = @event_type_id;
	
	DELETE [dbo].Event_Types WHERE Event_Type_ID = @event_type_id;
END
GO