USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_crds_Capacity_App_Data]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[api_crds_Capacity_App_Data] AS' 
END
GO
-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-05-05
-- Description:	Create api_crds_Capacity_App_Data to support the Capacity app
-- =============================================
ALTER PROCEDURE [dbo].api_crds_Capacity_App_Data 
	@EventId INT
AS
BEGIN

	DECLARE @RoomCapacityData TABLE
	(
		Event_Room_ID INT,
		Age_Bracket_Key NVARCHAR(50),
		Attendance INT,
		Capacity INT
	)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'NURSERY', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Jan Birthdate Groups
			176905, 176904, 176903, 176902, 176901, 176900, 176899, 176898, 176897, 176896, 176895, 176894,
		-- Feb Birthdate Groups
			176917, 176916, 176915, 176914, 176913, 176912, 176911, 176910, 176909, 176908, 176907, 176906,
		-- Mar Birthdate Groups
			176929, 176928, 176927, 176926, 176925, 176924, 176923, 176922, 176921, 176920, 176919, 176918,
		-- Apr Birthdate Groups
			176941, 176940, 176939, 176938, 176937, 176936, 176935, 176934, 176933, 176932, 176931, 176930,
		-- May Birthday Groups
			176953, 176952, 176951, 176950, 176949, 176948, 176947, 176946, 176945, 176944, 176943, 176942,
		-- Jun Birthday Groups
			176965, 176964, 176963, 176962, 176961, 176960, 176959, 176958, 176957, 176956, 176955, 176954,
		-- Jul Birthday Groups
			176977, 176976, 176975, 176974, 176973, 176972, 176971, 176970, 176969, 176968, 176967, 176966,
		-- Aug Birthday Groups
			176989, 176988, 176987, 176986, 176985, 176984, 176983, 176982, 176981, 176980, 176979, 176978,
		-- Sep Birthday Groups
			177001, 177000, 176999, 176998, 176997, 176996, 176995, 176994, 176993, 176992, 176991, 176990,
		-- Oct Birthday Groups
			177013, 177012, 177011, 177010, 177009, 177008, 177007, 177006, 177005, 177004, 177003, 177002,
		-- Nov Birthday Groups
			177025, 177024, 177023, 177022, 177021, 177020, 177019, 177018, 177017, 177016, 177015, 177014,
		-- Dec Birthday Groups
			177037, 177036, 177035, 177034, 177033, 177032, 177031, 177030, 177029, 177028, 177027, 177026
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'FIRST_YR', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- First Year Groups
			173988, 173989, 173990, 173991, 173992, 173993, 173994, 173995, 173996, 173997, 173998, 173999
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'SECOND_YR', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Second Year Groups
			173976, 173977, 173978, 173979, 173980, 173981, 173982, 173983, 173984, 173985, 173986, 173987
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'THIRD_YR', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Third Year Groups
			173964, 173965, 173966, 173967, 173968, 173969, 173970, 173971, 173972, 173973, 173974, 173975
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'FOURTH_YR', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Fourth Year Groups
			173952, 173953, 173954, 173955, 173956, 173957, 173958, 173959, 173960, 173961, 173962, 173963
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'FIFTH_YR', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Fifth Year Groups
			173940, 173941, 173942, 173943, 173944, 173945, 173946, 173947, 173948, 173949, 173950, 173951
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'KINDERGARTEN', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Kindergarten Groups
			173939
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'FIRST_GRADE', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- First Grade Groups
			173938
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'SECOND_GRADE', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Second Grade Groups
			173937
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'THIRD_GRADE', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Third Grade Groups
			173936
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'FOURTH_GRADE', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Fourth Grade Groups
			173935
		)

	INSERT INTO @RoomCapacityData (er.Event_Room_ID, Age_Bracket_Key, Attendance, Capacity)
		SELECT DISTINCT er.Event_Room_ID, 'FIFTH_GRADE', 
		([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
		[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance', 
		er.Capacity FROM event_groups eg inner join Event_Rooms er on eg.event_room_id=er.Event_Room_ID 
		where eg.event_id=@EventId and eg.group_id in (
		-- Fifth Grade Groups
			173934
		)

	DECLARE @Event_Room_ID INT
	DECLARE @Age_Bracket_Key NVARCHAR(20)
	DECLARE room_cursor CURSOR FOR SELECT Event_Room_ID, Age_Bracket_Key FROM @RoomCapacityData

	OPEN room_cursor
	FETCH NEXT FROM room_cursor INTO @Event_Room_ID, @Age_Bracket_Key

	WHILE @@FETCH_STATUS = 0
	BEGIN
		-- JPC - this is a little kludgy, might not hurt to explore refactoring if we think this is the correct approach 
		DECLARE @Capacity INT = 0
		DECLARE @Attendance INT = 0

		SELECT @Capacity = (select SUM(Capacity) from cr_Bumping_Rules br 
			INNER JOIN event_rooms er on br.to_event_room_id=er.Event_Room_ID where er.Event_ID=@EventId and er.Event_Room_ID != @Event_Room_ID
			and br.From_Event_Room_ID = @Event_Room_ID
			and er.Capacity IS NOT NULL)

		SELECT @Attendance = (
			select SUM([dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 3) +
			[dbo].crds_getEventParticipantStatusCount(ISNULL(@EventId, er.Room_ID), er.Room_ID, 4)) AS 'Attendance' 
			from cr_Bumping_Rules br INNER JOIN event_rooms er on br.to_event_room_id=er.Event_Room_ID where er.Event_ID=@EventId)

		UPDATE @RoomCapacityData SET Capacity = Capacity + @Capacity WHERE Event_Room_ID = @Event_Room_ID AND Age_Bracket_Key = @Age_Bracket_Key AND @Capacity IS NOT NULL
		UPDATE @RoomCapacityData SET Attendance = Attendance + @Attendance WHERE Event_Room_ID = @Event_Room_ID AND Age_Bracket_Key = @Age_Bracket_Key AND @Attendance IS NOT NULL

		FETCH NEXT FROM room_cursor INTO @Event_Room_ID, @Age_Bracket_Key
	END

	CLOSE room_cursor
	DEALLOCATE room_cursor

	SELECT * FROM @RoomCapacityData

END

IF NOT EXISTS (SELECT 1 FROM [dbo].[dp_API_Procedures] WHERE [Procedure_Name] = N'api_crds_Capacity_App_Data')
BEGIN 
	INSERT INTO [dbo].[dp_API_Procedures] (
		 Procedure_Name
		,Description
	) VALUES (
		 N'api_crds_Capacity_App_Data'
		,N'Returns attendance and capacity for KC groups'
	)
END
