-- =============================================
-- Author:      Tim Giblin
-- Create date: 2017-6-16
-- Description:	Catch Foreign Key Constraint and pass back up so that we can send back down to user
-- =============================================

USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[api_crds_Update_Single_Room_Checkin_Data] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_crds_Update_Single_Room_Checkin_Data]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[api_crds_Update_Single_Room_Checkin_Data] AS'
END
GO
USE [MinistryPlatform]
GO
/****** Object:  StoredProcedure [dbo].[api_crds_Update_Single_Room_Checkin_Data]    Script Date: 3/16/2017 10:09:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[api_crds_Update_Single_Room_Checkin_Data]
	@EventId INT,
	@RoomId INT,
	@GroupsXml XML
AS
BEGIN

-- Default values for a new event room
DECLARE @Allow_Checkin INT = 0
DECLARE @Capacity INT = 0
DECLARE @Volunteers INT = 0

-- Decalred here, set below for the moment - not sure this needs to come in from the proc call
DECLARE @EventRoomId INT

DECLARE @AdventureClubEventTypeID INT = 20

-- These fields are for the event room, and will eventually come down as params
DECLARE @EventRoomAdventureClub BIT = 0

-- 1. Get the event data for the Event ID - either the parent or the ac event
DECLARE @Events TABLE
(
	Event_ID int,
	Event_Type_ID int,
	Parent_Event_ID int
)

-- this is not working right now, because we need to get the event id...
INSERT INTO @Events (Event_ID, Event_Type_ID, Parent_Event_ID)
	SELECT DISTINCT Event_ID, Event_Type_ID, Parent_Event_ID
	FROM [dbo].[Events]
	WHERE Event_ID = @EventID OR Parent_Event_ID = @EventID

-- get parent events
INSERT INTO @Events(Event_ID, Event_Type_ID, Parent_Event_ID)
	SELECT DISTINCT Event_Id, Event_Type_ID, Parent_Event_ID
	FROM [dbo].[Events]
	WHERE Event_ID IN (SELECT Parent_Event_ID FROM @Events)

DECLARE @InactiveEventRoomId INT = 0

-- delete the event room off the other event (AC or non-AC)
SELECT @InactiveEventRoomId = Event_Room_ID,
	@Allow_Checkin = ISNULL(er.Allow_Checkin, @Allow_Checkin),
	@Capacity = ISNULL(er.Capacity, @Capacity),
	@Volunteers = ISNULL(er.Volunteers, @Volunteers)
FROM Event_Rooms er
WHERE er.Event_ID = (SELECT TOP(1) Event_ID FROM @Events WHERE Event_ID <> @EventId) AND Room_ID = @RoomID

DELETE FROM Event_Groups WHERE Event_Room_ID = @InactiveEventRoomId

DELETE FROM Event_Rooms WHERE Event_Room_ID = @InactiveEventRoomId

-- Create the Event Room, if needed, or just get the event room ID
IF NOT EXISTS (SELECT * FROM Event_Rooms WHERE Event_ID IN (SELECT Event_ID FROM @Events) AND Room_ID=@RoomID)
BEGIN
	INSERT INTO Event_Rooms
		([Event_ID], [Room_ID], [Domain_ID], [Hidden], [Allow_Checkin], [Capacity], [Volunteers])
	VALUES
		(@EventId, @RoomId, 1, 0, @Allow_Checkin, @Capacity, @Volunteers)

	SELECT @EventRoomId = Event_Room_ID FROM Event_Rooms WHERE Event_Room_ID = SCOPE_IDENTITY()
END
ELSE
	BEGIN
		SELECT @EventRoomId = Event_Room_ID FROM Event_Rooms WHERE Event_ID IN (SELECT Event_ID FROM @Events) AND Room_ID = @RoomId
	END

-- delete all groups on the event room before re-adding
DELETE FROM Event_Groups WHERE Event_Room_ID = @EventRoomId AND Event_ID = @EventId AND Room_ID = @RoomId

-- all group ids are going into this table
DECLARE @AllGroupIds TABLE
(
	Group_ID INT
)

-- groups already on the event
DECLARE @ExtantGroupIds TABLE
(
	GroupId INT
)

INSERT INTO @ExtantGroupIds
SELECT Group_ID
FROM Event_Groups WHERE Event_ID = @EventId AND Room_ID = @RoomId

-----------------------------------
---- Get Nursery Group IDs
-----------------------------------
DECLARE @NurseryGroups TABLE
(
	Id INT,
	TypeId INT,
	Selected BIT
)

INSERT INTO @NurseryGroups (Id, TypeId, Selected)
SELECT DISTINCT
	'Id' = x.v.value('Id[1]', 'int'),
	'TypeId' = x.v.value('TypeId[1]', 'int'),
	'Selected' = x.v.value('Selected[1]', 'bit')
FROM
	@GroupsXml.nodes('Groups/NurseryGroupXml/Attribute') x(v)

-- Get the nursery group ids for selected groups that are not already on the event
INSERT INTO @AllGroupIds SELECT Group_ID FROM Attributes a INNER JOIN Group_Attributes ga ON a.Attribute_ID = ga.Attribute_ID
WHERE a.Attribute_ID IN (SELECT Id FROM @NurseryGroups ng WHERE Selected=1) AND ga.Group_ID NOT IN (SELECT * FROM @ExtantGroupIds)

-------------------------------
---- Get Age Group IDs
-------------------------------

DECLARE @AgeGroups TABLE
(
	YearId INT,
	YearTypeId INT,
	MonthId INT,
	MonthTypeId INT,
	Selected BIT
)

INSERT INTO @AgeGroups (YearId, YearTypeId, MonthId, MonthTypeId, Selected)
SELECT DISTINCT
	'YearId' = x.v.value('YearId[1]', 'int'),
	'YearTypeId' = x.v.value('YearTypeId[1]', 'int'),
	'MonthId' = x.v.value('MonthId[1]', 'int'),
	'MonthTypeId' = x.v.value('MonthTypeId[1]', 'int'),
	'Selected' = x.v.value('Selected[1]', 'bit')
FROM
	@GroupsXml.nodes('Groups/YearGroupXml/Attribute') x(v)

-- Add the new event groups for all group types here
DECLARE @YearId INT
DECLARE @MonthId INT
DECLARE @Selected BIT

DECLARE id_cursor CURSOR FOR SELECT YearId, MonthId, Selected FROM @AgeGroups

OPEN id_cursor
FETCH NEXT FROM id_cursor INTO @YearId, @MonthId, @Selected

WHILE @@FETCH_STATUS = 0
BEGIN
	IF (@Selected = 1)
	BEGIN
		INSERT INTO @AllGroupIds
		SELECT g1.Group_ID
		FROM Group_Attributes g1, Group_Attributes g2 WHERE g1.Group_ID = g2.Group_ID and g1.Attribute_ID=@YearId and g2.Attribute_ID=@MonthId
	END

	FETCH NEXT FROM id_cursor INTO @YearId, @MonthId, @Selected
END

CLOSE id_cursor
DEALLOCATE id_cursor

----------------------------
---- Get Grade Group Ids
----------------------------

DECLARE @GradeGroups TABLE
(
	GradeYearId INT,
	GradeYearTypeId INT,
	GradeSelected BIT
)

INSERT INTO @GradeGroups (GradeYearId, GradeYearTypeId, GradeSelected)
SELECT DISTINCT
	'GradeYearId' = x.v.value('Id[1]', 'int'),
	'GradeYearTypeId' = x.v.value('TypeId[1]', 'int'),
	'GradeSelected' = x.v.value('Selected[1]', 'bit')
FROM
	@GroupsXml.nodes('Groups/GradeGroupXml/Attribute') x(v)

-- Add the new event groups for all group types here
DECLARE @GradeYearId INT
DECLARE @GradeSelected BIT

DECLARE @LogTable TABLE
(
	YearGradeId INT,
	GradeSelected BIT
)

DECLARE grade_cursor CURSOR FOR SELECT GradeYearId, GradeSelected FROM @GradeGroups

OPEN grade_cursor
FETCH NEXT FROM grade_cursor INTO @GradeYearId, @GradeSelected

INSERT INTO @LogTable
SELECT @GradeYearId, @GradeSelected

WHILE @@FETCH_STATUS = 0
BEGIN

	IF (@GradeSelected = 1)
	BEGIN
		INSERT INTO @AllGroupIds
		SELECT Group_ID
		FROM Group_Attributes WHERE Attribute_ID=@GradeYearId
	END

	FETCH NEXT FROM grade_cursor INTO @GradeYearId, @GradeSelected
END

CLOSE grade_cursor
DEALLOCATE grade_cursor

-----------------------------
---- Create All Event Groups Here
-----------------------------

-- Add the new event groups for all group types here
DECLARE @Group_ID INT
DECLARE group_cursor CURSOR FOR SELECT Group_ID FROM @AllGroupIds

OPEN group_cursor
FETCH NEXT FROM group_cursor INTO @Group_ID

DECLARE @IS_ERROR bit
Set @IS_ERROR = 0
DECLARE @Duplicate_Group_Ids VARCHAR(1000)
Set @Duplicate_Group_Ids = ''

WHILE @@FETCH_STATUS = 0
BEGIN
	BEGIN TRY
		INSERT INTO Event_Groups (Group_ID, Event_ID, Room_ID, Domain_ID, Event_Room_ID)
		VALUES (@Group_ID, @EventId, @RoomId, 1, @EventRoomId)
	END TRY
	BEGIN CATCH
		IF ERROR_NUMBER() = 2627
			SET @Is_Error = 1
			SET @Duplicate_Group_Ids = CONCAT(@Duplicate_Group_Ids, CAST(@Group_ID AS VARCHAR(100)), ',')
	END CATCH
	FETCH NEXT FROM group_cursor INTO @Group_ID
END

IF @Is_Error = 1
	RAISERROR('%s', 16, 1, @Duplicate_Group_Ids)

CLOSE group_cursor
DEALLOCATE group_cursor

-----------------------------------
---- Set adventure club status here
---- TG 3/16 commented this out
-----------------------------------

-- Correctly set adventure club status - if there are existing event rooms on the AC event, uncancel the AC event -
-- Do not do the reverse, as it will cancel AC rooms due to an MP trigger
-- IF EXISTS (SELECT * FROM Event_Rooms WHERE @EventId = (SELECT Event_ID FROM @Events WHERE Event_Type_ID = @AdventureClubEventTypeID))
-- BEGIN
-- 	UPDATE [Events] SET Cancelled = 0 WHERE Event_ID = (SELECT Event_ID FROM @Events WHERE Event_Type_ID = @AdventureClubEventTypeID)
-- END

-- return the event room data from the proc
SELECT TOP(1) * FROM Event_Rooms WHERE Event_Room_ID = @EventRoomId
SELECT * FROM @Events

END
GO
