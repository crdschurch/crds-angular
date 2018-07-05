USE [MinistryPlatform]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Henney, Sarah
-- Create date:	07/03/2018
-- Description:	Stored procedure declaration for deleting room data
-- =============================================

-- Defines cr_QA_Delete_Room
IF NOT EXISTS ( SELECT  *
	FROM    sys.objects
	WHERE   object_id = OBJECT_ID(N'cr_QA_Delete_Room')
			AND type IN ( N'P', N'PC' ) )
	EXEC('CREATE PROCEDURE dbo.cr_QA_Delete_Room
	@room_id int AS SET NOCOUNT ON;')
GO
ALTER PROCEDURE [dbo].[cr_QA_Delete_Room] 
	@room_id int
AS
BEGIN
	SET NOCOUNT ON;

	IF @room_id is null
		RETURN;
	
	--Delete foreign key entries that can't be nullified
	DELETE [dbo].cr_Kiosk_Configs WHERE Room_ID = @room_id;
	DELETE [dbo].Group_Rooms WHERE Room_ID = @room_id;

	--Delete Equipment
	DECLARE @equipment_to_delete TABLE
	(
		equipment_id int
	)
	INSERT INTO @equipment_to_delete (equipment_id) SELECT Equipment_ID 
		FROM [dbo].Equipment WHERE Room_ID = @room_id;

	DECLARE @cur_entry_id int = 0;

	WHILE @cur_entry_id is not null
	BEGIN
		--Get top item in list
		Set @cur_entry_id = (SELECT TOP 1 equipment_id 
			FROM @equipment_to_delete
			WHERE equipment_id > @cur_entry_id
			ORDER BY equipment_id ASC);

		--Delete using the stored proc
		IF @cur_entry_id is not null
		BEGIN
			EXEC [dbo].[cr_QA_Delete_Equipment] @cur_entry_id;
		END
	END

	--Delete Event_Rooms
	DECLARE @event_rooms_to_delete TABLE
	(
		event_room_id int
	)
	INSERT INTO @event_rooms_to_delete (event_room_id) SELECT Event_Room_ID 
		FROM [dbo].Event_Rooms WHERE Room_ID = @room_id;

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

	--Nullify foreign keys
	UPDATE [dbo].Event_Equipment SET Room_ID = null WHERE Room_ID = @room_id;
	UPDATE [dbo].Event_Groups SET Room_ID = null WHERE Room_ID = @room_id;
	UPDATE [dbo].Event_Participants SET Room_ID = null WHERE Room_ID = @room_id;
	UPDATE [dbo].Program_Groups SET Room_ID = null WHERE Room_ID = @room_id;
	UPDATE [dbo].Room_Layouts SET Room_ID = null WHERE Room_ID = @room_id;
	UPDATE [dbo].Rooms SET Parent_Room_ID = null WHERE Parent_Room_ID = @room_id;
	
	DELETE [dbo].Rooms WHERE Room_ID = @room_id;
END
GO