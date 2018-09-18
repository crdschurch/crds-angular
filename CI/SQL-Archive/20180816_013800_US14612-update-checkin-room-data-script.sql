-- =============================================
-- Author:      John Cleaver
-- Create date: 2018-08-16
-- Description:	Identify Correct Column for Group_ID FK
-- =============================================

USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[api_crds_Get_Checkin_Room_Data]    Script Date: 1/6/2017 12:06:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_crds_Get_Checkin_Room_Data]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[api_crds_Get_Checkin_Room_Data] AS'
END
GO

ALTER PROCEDURE [dbo].[api_crds_Get_Checkin_Room_Data]

	@EventID int

AS
BEGIN

--DECLARE @EventId INT = 4534873
DECLARE @LocationId INT

DECLARE @AgeAttributeTypeId INT = 102
DECLARE @BirthMonthAttributeTypeId INT = 103
DECLARE @GradesAttributeTypeId INT = 104
DECLARE @NurseryMonthsAttributeTypeId INT = 105

DECLARE @RoomUsageKidsClubTypeId INT = 6

SELECT @LocationId = Location_ID FROM Congregations WHERE Congregation_ID = (SELECT Congregation_ID FROM Events WHERE Event_ID = @EventID)

-- 1. Get the event and subevent
DECLARE @Events TABLE
(
	Event_ID int
)

INSERT INTO @Events (Event_ID)
	SELECT DISTINCT Event_ID
	FROM [dbo].[Events]
	WHERE Event_ID = @EventID OR Parent_Event_ID = @EventID
	AND Cancelled = 0

DECLARE @TempEventRooms TABLE
(
	Allow_Checkin BIT,
	Capacity INT DEFAULT 0,
	Checked_In INT,
	Event_ID INT,
	Event_Room_ID INT NULL,
	Room_ID INT,
	Room_Name VARCHAR(50),
	KC_Sort_Order INT NULL,
	Signed_In INT,
	Volunteers INT
)

-- this is the rooms only, populated with the default values and room id/room name
INSERT INTO @TempEventRooms (Event_ID, Room_ID, Room_Name, KC_Sort_Order)
	SELECT @EventId, r.Room_ID, Room_Name, KC_Sort_Order
	FROM Buildings b INNER JOIN Rooms r ON b.Building_ID = r.Building_ID
	WHERE r.Room_Usage_Type_ID = @RoomUsageKidsClubTypeId
	AND b.Location_ID = @LocationId

DECLARE @Event_Room_Event_ID INT
DECLARE @Event_Room_ID INT
DECLARE @AllowCheckin BIT
DECLARE @Capacity INT
DECLARE @Volunteers INT

-- Cursor is used here to get around issues that came up with joining on multiple columns
DECLARE @Room_ID INT
DECLARE room_cursor CURSOR FOR SELECT Room_ID FROM @TempEventRooms

OPEN room_cursor
FETCH NEXT FROM room_cursor INTO @Room_ID

WHILE @@FETCH_STATUS = 0
BEGIN

	SET @Event_Room_Event_ID = NULL
	SET @Event_Room_ID = NULL
	SET @AllowCheckin = 0
	SET @Capacity = 0
	SET @Volunteers = 0

	SELECT
		@Event_Room_Event_ID = Event_ID,
		@Event_Room_ID = Event_Room_ID,
		@AllowCheckin = Allow_Checkin,
		@Capacity = Capacity,
		@Volunteers = Volunteers
	FROM Event_Rooms WHERE Room_ID = @Room_ID AND Event_ID IN (SELECT Event_ID FROM @Events)

	UPDATE @TempEventRooms SET
		Event_ID = ISNULL(@Event_Room_Event_ID, @EventID),
		Event_Room_ID = @Event_Room_ID,
		Allow_Checkin = @AllowCheckin,
		Capacity = ISNULL(@Capacity, 0),
		Volunteers = ISNULL(@Volunteers, 0),
		Checked_In = [dbo].crds_getEventParticipantStatusCount(ISNULL(@Event_Room_Event_ID, @EventID), @Room_ID, 4),
		Signed_In = [dbo].crds_getEventParticipantStatusCount(ISNULL(@Event_Room_Event_ID, @EventID), @Room_ID, 3)
	WHERE Room_ID = @Room_ID

	FETCH NEXT FROM room_cursor INTO @Room_ID
END

CLOSE room_cursor
DEALLOCATE room_cursor

-- Get the event groups for the events
DECLARE @EventGroups TABLE
(
	Event_ID int,
	Group_ID int,
	Event_Group_ID int,
	Event_Room_ID int,
	Room_ID int,
	Signed_In int,
	Checked_In int
)

-- get event groups where the group is tied to an event and has a room reservation,
-- probably grab the attributes here as well
INSERT INTO @EventGroups (Event_ID, Group_ID, Event_Group_ID, Event_Room_ID, Room_ID, Checked_In, Signed_In)
	SELECT
		eg.Event_ID,
		eg.Group_ID,
		Event_Group_ID,
		er.Event_Room_ID,
		er.Room_ID,
		Checked_In = [dbo].crds_getEventParticipantStatusCount(@EventID, @Room_ID, 4),
		Signed_In = [dbo].crds_getEventParticipantStatusCount(@EventID, @Room_ID, 3)
	FROM Event_Groups eg INNER JOIN Event_Rooms er on eg.Event_Room_ID = er.Event_Room_ID
	WHERE eg.Event_ID IN (SELECT Event_ID FROM @Events)

DECLARE @GroupAttributes TABLE
(
	Group_ID int,
	Attribute_Type_ID int,
	Attribute_ID int,
	Attribute_Name NVARCHAR(100),
	Sort_Order int
)

INSERT INTO @GroupAttributes (Group_ID, Attribute_Type_ID, Attribute_ID, Attribute_Name, Sort_Order)
	SELECT Group_ID, Attribute_Type_ID, a.Attribute_ID, Attribute_Name, Sort_Order
	FROM Group_Attributes ga INNER JOIN Attributes a ON ga.Attribute_ID = a.Attribute_ID
	WHERE ga.Group_ID IN (SELECT Group_ID FROM @EventGroups)

SELECT * FROM @TempEventRooms

SELECT * FROM @EventGroups

SELECT * FROM @GroupAttributes

SELECT a.Attribute_ID, a.Attribute_Name, a.Description, a.Attribute_Type_ID, at.Description as Attribute_Type_Name FROM Attributes a INNER JOIN Attribute_Types at ON a.Attribute_Type_ID = at.Attribute_Type_ID
WHERE at.Attribute_Type_ID IN (@AgeAttributeTypeId, @BirthMonthAttributeTypeId, @GradesAttributeTypeId, @NurseryMonthsAttributeTypeId)

END
