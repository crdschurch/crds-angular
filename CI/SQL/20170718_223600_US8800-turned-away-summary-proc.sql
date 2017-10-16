USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Turned_Away_Summary]    Script Date: 7/18/2017 7:50:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Turned_Away_Summary]   Script Date: 6/23/2017 8:43:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ===============================================================
-- Author: John Cleaver	
-- Create date: 7/18/2017
-- Description:	Create [report_CRDS_Turned_Away_Summary] proc to show number
-- of kids by group that were not able to check into a KC event
-- ===============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[report_CRDS_Turned_Away_Summary]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[report_CRDS_Turned_Away_Summary] AS' 
END
GO

ALTER PROCEDURE [dbo].[report_CRDS_Turned_Away_Summary]

      @StartDate DATETIME,
	  @EndDate DATETIME,
	  @EventCongregations NVARCHAR(1000)
AS
	DECLARE @GroupsIdMaps TABLE
	(
		Group_ID INT,
		GroupName NVARCHAR(100),
		GroupSetId INT
	)

	-- this horrible, horrible approach was done because we don't have group types specific enough for years and trying to parse
	-- it out of a name is asking for trouble. Note that for purposes of making sure all the values are there,
	-- these are sequential for all the age groups
	INSERT INTO @GroupsIdMaps ([Group_ID], [GroupName], [GroupSetId]) VALUES
	(176894, 'Kids Club 0 Yr', 0),
	(176895, 'Kids Club 0 Yr', 0),
	(176896, 'Kids Club 0 Yr', 0),
	(176897, 'Kids Club 0 Yr', 0),
	(176898, 'Kids Club 0 Yr', 0),
	(176899, 'Kids Club 0 Yr', 0),
	(176900, 'Kids Club 0 Yr', 0),
	(176901, 'Kids Club 0 Yr', 0),
	(176902, 'Kids Club 0 Yr', 0),
	(176903, 'Kids Club 0 Yr', 0),
	(176904, 'Kids Club 0 Yr', 0),
	(176905, 'Kids Club 0 Yr', 0),
	(176906, 'Kids Club 0 Yr', 0),
	(176907, 'Kids Club 0 Yr', 0),
	(176908, 'Kids Club 0 Yr', 0),
	(176909, 'Kids Club 0 Yr', 0),
	(176910, 'Kids Club 0 Yr', 0),
	(176911, 'Kids Club 0 Yr', 0),
	(176912, 'Kids Club 0 Yr', 0),
	(176913, 'Kids Club 0 Yr', 0),
	(176914, 'Kids Club 0 Yr', 0),
	(176915, 'Kids Club 0 Yr', 0),
	(176916, 'Kids Club 0 Yr', 0),
	(176917, 'Kids Club 0 Yr', 0),
	(176918, 'Kids Club 0 Yr', 0),
	(176919, 'Kids Club 0 Yr', 0),
	(176920, 'Kids Club 0 Yr', 0),
	(176921, 'Kids Club 0 Yr', 0),
	(176922, 'Kids Club 0 Yr', 0),
	(176923, 'Kids Club 0 Yr', 0),
	(176924, 'Kids Club 0 Yr', 0),
	(176925, 'Kids Club 0 Yr', 0),
	(176926, 'Kids Club 0 Yr', 0),
	(176927, 'Kids Club 0 Yr', 0),
	(176928, 'Kids Club 0 Yr', 0),
	(176929, 'Kids Club 0 Yr', 0),
	(176930, 'Kids Club 0 Yr', 0),
	(176931, 'Kids Club 0 Yr', 0),
	(176932, 'Kids Club 0 Yr', 0),
	(176933, 'Kids Club 0 Yr', 0),
	(176934, 'Kids Club 0 Yr', 0),
	(176935, 'Kids Club 0 Yr', 0),
	(176936, 'Kids Club 0 Yr', 0),
	(176937, 'Kids Club 0 Yr', 0),
	(176938, 'Kids Club 0 Yr', 0),
	(176939, 'Kids Club 0 Yr', 0),
	(176940, 'Kids Club 0 Yr', 0),
	(176941, 'Kids Club 0 Yr', 0),
	(176942, 'Kids Club 0 Yr', 0),
	(176943, 'Kids Club 0 Yr', 0),
	(176944, 'Kids Club 0 Yr', 0),
	(176945, 'Kids Club 0 Yr', 0),		
	(176946, 'Kids Club 0 Yr', 0),			
	(176947, 'Kids Club 0 Yr', 0),	
	(176948, 'Kids Club 0 Yr', 0),
	(176949, 'Kids Club 0 Yr', 0),		
	(176950, 'Kids Club 0 Yr', 0),
	(176951, 'Kids Club 0 Yr', 0),
	(176952, 'Kids Club 0 Yr', 0),
	(176953, 'Kids Club 0 Yr', 0),
	(176954, 'Kids Club 0 Yr', 0),
	(176955, 'Kids Club 0 Yr', 0),
	(176956, 'Kids Club 0 Yr', 0),
	(176957, 'Kids Club 0 Yr', 0),
	(176958, 'Kids Club 0 Yr', 0),
	(176959, 'Kids Club 0 Yr', 0),
	(176960, 'Kids Club 0 Yr', 0),
	(176961, 'Kids Club 0 Yr', 0),
	(176962, 'Kids Club 0 Yr', 0),
	(176963, 'Kids Club 0 Yr', 0),
	(176964, 'Kids Club 0 Yr', 0),
	(176965, 'Kids Club 0 Yr', 0),
	(176966, 'Kids Club 0 Yr', 0),
	(176967, 'Kids Club 0 Yr', 0),
	(176968, 'Kids Club 0 Yr', 0),
	(176969, 'Kids Club 0 Yr', 0),
	(176970, 'Kids Club 0 Yr', 0),
	(176971, 'Kids Club 0 Yr', 0),
	(176972, 'Kids Club 0 Yr', 0),
	(176973, 'Kids Club 0 Yr', 0),
	(176974, 'Kids Club 0 Yr', 0),
	(176975, 'Kids Club 0 Yr', 0),
	(176976, 'Kids Club 0 Yr', 0),
	(176977, 'Kids Club 0 Yr', 0),
	(176978, 'Kids Club 0 Yr', 0),
	(176979, 'Kids Club 0 Yr', 0),
	(176980, 'Kids Club 0 Yr', 0),
	(176981, 'Kids Club 0 Yr', 0),
	(176982, 'Kids Club 0 Yr', 0),
	(176983, 'Kids Club 0 Yr', 0),
	(176984, 'Kids Club 0 Yr', 0),
	(176985, 'Kids Club 0 Yr', 0),
	(176986, 'Kids Club 0 Yr', 0),
	(176987, 'Kids Club 0 Yr', 0),
	(176988, 'Kids Club 0 Yr', 0),
	(176989, 'Kids Club 0 Yr', 0),
	(176990, 'Kids Club 0 Yr', 0),
	(176991, 'Kids Club 0 Yr', 0),
	(176992, 'Kids Club 0 Yr', 0),
	(176993, 'Kids Club 0 Yr', 0),
	(176994, 'Kids Club 0 Yr', 0),
	(176995, 'Kids Club 0 Yr', 0),
	(176996, 'Kids Club 0 Yr', 0),
	(176997, 'Kids Club 0 Yr', 0),
	(176998, 'Kids Club 0 Yr', 0),
	(176999, 'Kids Club 0 Yr', 0),
	(177000, 'Kids Club 0 Yr', 0),
	(177001, 'Kids Club 0 Yr', 0),
	(177002, 'Kids Club 0 Yr', 0),
	(177003, 'Kids Club 0 Yr', 0),
	(177004, 'Kids Club 0 Yr', 0),
	(177005, 'Kids Club 0 Yr', 0),
	(177006, 'Kids Club 0 Yr', 0),
	(177007, 'Kids Club 0 Yr', 0),
	(177008, 'Kids Club 0 Yr', 0),
	(177009, 'Kids Club 0 Yr', 0),
	(177010, 'Kids Club 0 Yr', 0),
	(177011, 'Kids Club 0 Yr', 0),
	(177012, 'Kids Club 0 Yr', 0),
	(177013, 'Kids Club 0 Yr', 0),
	(177014, 'Kids Club 0 Yr', 0),
	(177015, 'Kids Club 0 Yr', 0),
	(177016, 'Kids Club 0 Yr', 0),
	(177017, 'Kids Club 0 Yr', 0),
	(177018, 'Kids Club 0 Yr', 0),
	(177019, 'Kids Club 0 Yr', 0),
	(177020, 'Kids Club 0 Yr', 0),
	(177021, 'Kids Club 0 Yr', 0),
	(177022, 'Kids Club 0 Yr', 0),
	(177023, 'Kids Club 0 Yr', 0),
	(177024, 'Kids Club 0 Yr', 0),
	(177025, 'Kids Club 0 Yr', 0),
	(177026, 'Kids Club 0 Yr', 0),
	(177027, 'Kids Club 0 Yr', 0),
	(177028, 'Kids Club 0 Yr', 0),
	(177029, 'Kids Club 0 Yr', 0),
	(177030, 'Kids Club 0 Yr', 0),
	(177031, 'Kids Club 0 Yr', 0),
	(177032, 'Kids Club 0 Yr', 0),
	(177033, 'Kids Club 0 Yr', 0),
	(177034, 'Kids Club 0 Yr', 0),
	(177035, 'Kids Club 0 Yr', 0),
	(177036, 'Kids Club 0 Yr', 0),
	(177037, 'Kids Club 0 Yr', 0),
	
	(173988, 'Kids Club 1 Yr', 1),
	(173989, 'Kids Club 1 Yr', 1),
	(173990, 'Kids Club 1 Yr', 1),
	(173991, 'Kids Club 1 Yr', 1),
	(173992, 'Kids Club 1 Yr', 1),
	(173993, 'Kids Club 1 Yr', 1),
	(173994, 'Kids Club 1 Yr', 1),
	(173995, 'Kids Club 1 Yr', 1),
	(173996, 'Kids Club 1 Yr', 1),
	(173997, 'Kids Club 1 Yr', 1),
	(173998, 'Kids Club 1 Yr', 1),
	(173999, 'Kids Club 1 Yr', 1),
	
	(173976, 'Kids Club 2 Yr', 2),
	(173977, 'Kids Club 2 Yr', 2),
	(173978, 'Kids Club 2 Yr', 2),
	(173979, 'Kids Club 2 Yr', 2),
	(173980, 'Kids Club 2 Yr', 2),
	(173981, 'Kids Club 2 Yr', 2),
	(173982, 'Kids Club 2 Yr', 2),
	(173983, 'Kids Club 2 Yr', 2),
	(173984, 'Kids Club 2 Yr', 2),
	(173985, 'Kids Club 2 Yr', 2),
	(173986, 'Kids Club 2 Yr', 2),
	(173987, 'Kids Club 2 Yr', 2),
	
	(173964, 'Kids Club 3 Yr', 3),
	(173965, 'Kids Club 3 Yr', 3),
	(173966, 'Kids Club 3 Yr', 3),
	(173967, 'Kids Club 3 Yr', 3),
	(173968, 'Kids Club 3 Yr', 3),
	(173969, 'Kids Club 3 Yr', 3),
	(173970, 'Kids Club 3 Yr', 3),
	(173971, 'Kids Club 3 Yr', 3),
	(173972, 'Kids Club 3 Yr', 3),
	(173973, 'Kids Club 3 Yr', 3),
	(173974, 'Kids Club 3 Yr', 3),
	(173975, 'Kids Club 3 Yr', 3),
	
	(173952, 'Kids Club 4 Yr', 4),
	(173953, 'Kids Club 4 Yr', 4),
	(173954, 'Kids Club 4 Yr', 4),
	(173955, 'Kids Club 4 Yr', 4),
	(173956, 'Kids Club 4 Yr', 4),
	(173957, 'Kids Club 4 Yr', 4),
	(173958, 'Kids Club 4 Yr', 4),
	(173959, 'Kids Club 4 Yr', 4),
	(173960, 'Kids Club 4 Yr', 4),
	(173961, 'Kids Club 4 Yr', 4),
	(173962, 'Kids Club 4 Yr', 4),
	(173963, 'Kids Club 4 Yr', 4),
	
	(173940, 'Kids Club 5+ Yr', 5),
	(173941, 'Kids Club 5+ Yr', 5),
	(173942, 'Kids Club 5+ Yr', 5),
	(173943, 'Kids Club 5+ Yr', 5),
	(173944, 'Kids Club 5+ Yr', 5),
	(173945, 'Kids Club 5+ Yr', 5),
	(173946, 'Kids Club 5+ Yr', 5),
	(173947, 'Kids Club 5+ Yr', 5),
	(173948, 'Kids Club 5+ Yr', 5),
	(173949, 'Kids Club 5+ Yr', 5),
	(173950, 'Kids Club 5+ Yr', 5),
	(173951, 'Kids Club 5+ Yr', 5),
	
	(173939, 'Kids Club Kindergarten', 6),
	(173938, 'Kids Club First Grade', 7),
	(173937, 'Kids Club Second Grade', 8),
	(173936, 'Kids Club Third Grade', 9),
	(173935, 'Kids Club Fourth Grade', 10),
	(173934, 'Kids Club Fifth Grade', 11)

	DECLARE @RawGroups TABLE
	(
		Turned_Away_In_Group INT,
		Event_ID INT,
		Event_Start_Date DATETIME,
		Event_Title NVARCHAR(100),
		Group_Name NVARCHAR(100),
		Group_ID INT,
		Participation_Status_ID INT
	)

    INSERT INTO @RawGroups SELECT
	-- get the count of participants for that group line
	(SELECT COUNT(*) FROM (SELECT DISTINCT s_ep.Participant_ID FROM event_participants s_ep
				WHERE s_ep.participation_status_id IN (6, 7)
				AND s_ep.Group_ID = g.Group_ID
				AND s_ep.Event_ID = e.Event_ID
				AND NOT EXISTS (
					SELECT * FROM Event_Participants s2_ep WHERE s2_ep.Participant_ID = s_ep.Participant_ID
					AND s2_ep.Participation_Status_ID IN (3, 4, 5) 
					AND s2_ep.Event_ID = s_ep.Event_ID) 
				GROUP BY Participant_ID) AS items) AS 'Turned_Away_In_Group',
	e.Event_ID, 
	e.Event_Start_Date, 
	e.Event_Title,
	(SELECT TOP(1) GroupName FROM @GroupsIdMaps gim WHERE gim.Group_ID = g.Group_ID) AS Group_Name,
	(SELECT TOP(1) GroupSetId FROM @GroupsIdMaps gim WHERE gim.Group_ID = g.Group_ID) AS Group_ID,
	ep.Participation_Status_ID

	-- Get groups from an event where there are any turned away participants
	FROM [Events] e INNER JOIN Event_Participants ep ON e.Event_ID = ep.Event_ID
		INNER JOIN Groups g ON ep.Group_ID = g.Group_ID
	WHERE e.Congregation_ID IN (SELECT * FROM dbo.fnSplitString(@EventCongregations,','))
	AND CONVERT(date, e.Event_Start_Date) >= @StartDate
	AND CONVERT(date, e.Event_Start_Date) <= @EndDate
	AND ep.Participation_Status_ID IN (6, 7)
	AND NOT EXISTS (SELECT * FROM Event_Participants s_ep WHERE s_ep.Participant_ID = ep.Participant_ID
		AND s_ep.Participation_Status_ID IN (3, 4, 5) AND s_ep.Event_ID = e.Event_ID)
	AND e.[Allow_Check-in] = 1
	AND e.Event_Type_ID != 243
	Group by e.Event_Id, e.Event_Start_Date, e.Event_Title, g.Group_Name, ep.Participation_Status_ID, g.Group_ID
	ORDER BY e.Event_Start_Date, g.Group_Name, ep.Participation_Status_ID

	SELECT SUM(R.Turned_Away_In_Group), R.Event_Id, R.Event_Start_Date, R.Event_Title, R.Participation_Status_ID, R.Group_ID FROM @RawGroups R
	Group by R.Event_Id, R.Event_Start_Date, R.Event_Title, R.Group_Name, R.Participation_Status_ID, R.Group_ID
	ORDER BY R.Event_Start_Date, R.Group_Name, R.Participation_Status_ID
GO


