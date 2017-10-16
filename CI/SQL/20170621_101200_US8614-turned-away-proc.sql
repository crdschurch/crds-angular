USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Turned_Away]   Script Date: 5/30/2017 8:43:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ===============================================================
-- Author: John Cleaver	
-- Create date: 6/21/2017
-- Description:	Update [report_CRDS_Turned_Away] proc to enforce
-- uniqueness on kids, as well as adding household info
-- ===============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[report_CRDS_Turned_Away]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[report_CRDS_Turned_Away] AS' 
END
GO

ALTER PROCEDURE [dbo].report_CRDS_Turned_Away

      @StartDate DATETIME,
	  @EndDate DATETIME,
	  @EventCongregations NVARCHAR(1000)
AS
	SELECT * FROM (

    SELECT
	e.Event_ID, 
	e.Event_Start_Date, 
	e.Event_Title,
	ep.Participant_ID, 
	c.Display_Name, 
	ep.Participation_Status_ID,
	ep.Event_Participant_ID,
	(SELECT TOP(1) s_g.Group_Name FROM groups s_g INNER JOIN Group_Participants s_gp ON s_g.Group_ID = s_gp.Group_ID
		WHERE s_gp.Participant_Id = ep.Participant_ID AND s_g.Group_Type_ID = 4) AS 'Group Name',
	ep.Time_In,
	h.Household_Name,
	ep.Checkin_Phone,
	ROW_NUMBER() over (partition by ep.participant_id, ep.event_id order by ep.Time_In desc) as rn
		FROM events e INNER JOIN Event_Participants ep ON
		e.Event_ID = ep.Event_ID INNER JOIN Participants p ON ep.Participant_ID = p.Participant_ID
		INNER JOIN contacts c ON p.Contact_ID = c.Contact_ID
		INNER JOIN Households h ON h.Household_ID = ep.Checkin_Household_ID
		WHERE ep.Participation_Status_ID IN (6, 7)
		AND NOT EXISTS 
		(SELECT * from Event_Participants s_ep WHERE s_ep.Participant_ID = ep.Participant_ID
		AND s_ep.Participation_Status_ID IN (3, 4, 5) AND s_ep.Event_ID = e.Event_ID)
		AND e.Congregation_ID IN (SELECT * FROM dbo.fnSplitString(@EventCongregations,','))
		AND CONVERT(date, e.Event_Start_Date) >= @StartDate
		AND CONVERT(date, e.Event_Start_Date) <= @EndDate) AS T
	WHERE T.rn <= 1
	ORDER BY t.Event_Start_Date, t.[Group Name];

GO


