USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Turned_Away_Summary]   Script Date: 6/23/2017 8:43:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ===============================================================
-- Author: John Cleaver	
-- Create date: 6/01/2017
-- Description:	Create [report_CRDS_Turned_Away_Summary] proc to show number
-- of kids by group that were not able to check into a KC event
-- ===============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[report_CRDS_Turned_Away_Summary]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[report_CRDS_Turned_Away_Summary] AS' 
END
GO

ALTER PROCEDURE [dbo].report_CRDS_Turned_Away_Summary

      @StartDate DATETIME,
	  @EndDate DATETIME,
	  @EventCongregations NVARCHAR(1000)
AS
	DECLARE @TurnedAwaySummary TABLE
	(
		Event_ID int,
		Event_Start_Date datetime,
		Event_Title varchar(255),
		Participation_Status_ID int,
		Participant_ID int,
		ParticipantAge datetime,
		ParticipantAgeYear int
	)

    INSERT INTO @TurnedAwaySummary SELECT DISTINCT
	e.Event_ID, 
	e.Event_Start_Date, 
	e.Event_Title,
	ep.Participation_Status_ID,
	ep.Participant_ID,
	(SELECT s_c.Date_of_Birth FROM Participants s_p INNER JOIN Contacts s_c ON s_p.Contact_ID = s_c.Contact_ID
		WHERE s_p.Participant_ID = ep.Participant_ID),
	0

	-- Get groups from an event where there are any turned away participants
	FROM [Events] e INNER JOIN Event_Participants ep ON e.Event_ID = ep.Event_ID
	WHERE e.Congregation_ID IN (SELECT * FROM dbo.fnSplitString(@EventCongregations,','))
	AND CONVERT(date, e.Event_Start_Date) >= @StartDate
	AND CONVERT(date, e.Event_Start_Date) <= @EndDate
	AND ep.Participation_Status_ID IN (6, 7)
	AND NOT EXISTS (SELECT * FROM Event_Participants s_ep WHERE s_ep.Participant_ID = ep.Participant_ID
		AND s_ep.Participation_Status_ID IN (3, 4, 5) AND s_ep.Event_ID = e.Event_ID)
	AND e.[Allow_Check-in] = 1
	AND e.Event_Type_ID != 243

	-- set the age in years here
	UPDATE @TurnedAwaySummary SET ParticipantAgeYear = (SELECT DATEDIFF(YY, ParticipantAge, GETDATE()) - CASE WHEN RIGHT(CONVERT(VARCHAR(6), GETDATE(), 12), 4) >= 
               RIGHT(CONVERT(VARCHAR(6), ParticipantAge, 12), 4) THEN 0 ELSE 1 END)

	SELECT
		Event_ID,
		Event_Start_Date,
		Event_Title,
		Participation_Status_ID,
		ParticipantAgeYear,
		COUNT(ParticipantAgeYear) AS Total_In_Year
	FROM @TurnedAwaySummary
	WHERE ParticipantAge IS NOT NULL
	GROUP BY Event_Id, Event_Start_Date, Event_Title, ParticipantAgeYear, Participation_Status_ID
	ORDER BY Event_Start_Date, ParticipantAgeYear, Participation_Status_ID
GO



