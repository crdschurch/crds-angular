USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Childcare_Summary]   Script Date: 5/30/2017 8:43:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ===============================================================
-- Author: John Cleaver	
-- Create date: 6/01/2017
-- Description:	Refactor of [report_CRDS_Childcare_Summary] proc
-- ===============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[report_CRDS_Childcare_Summary]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[report_CRDS_Childcare_Summary] AS' 
END
GO

ALTER PROCEDURE [dbo].[report_CRDS_Childcare_Summary]

      @StartDate DATETIME,
	  @EndDate DATETIME,
	  @CongregationId INT

AS
    BEGIN
        SET NOCOUNT ON;
		SET @StartDate =  DATEADD(day, DATEDIFF(day, 0, @StartDate), '00:00:00');
		SET @EndDate =  DATEADD(day, DATEDIFF(day, 0, @EndDate), '23:59:00');
		
		DECLARE @AGE_GROUP_TYPE_ID int = 4;
		
		DECLARE @ChildcareSummary TABLE
		(
			Event_ID int,
			Group_Name varchar(255),
			EventDate datetime,
			StartTime datetime, 
			EndTime datetime,
			ParticipantID int,
			Age nvarchar(200),
			RSVP int,
			Checkin int,
			RSVPOnline int,
			RSVPOverride int
		)

		-- first case is children that had an rsvp
	INSERT INTO @ChildcareSummary SELECT DISTINCT
			e.Event_ID, -- Event_ID
			(select top(1) s_g.Group_Name from groups s_g inner join event_groups s_eg on s_g.Group_ID = s_eg.Group_ID inner join Group_Participants s_gp
				on s_g.Group_ID = s_gp.Group_ID where s_gp.Group_Participant_ID = gp.Enrolled_By), -- Group_Name    
			e.Event_Start_Date as EventDate, -- EventDate
			e.Event_Start_Date as StartTime, -- StartTime
			e.Event_End_Date as EndTime, -- EndTime
			p.Participant_ID, --- ParticipantID
			(SELECT TOP(1) Group_Name from Groups s_g inner join Group_Participants s_gp on s_g.Group_ID = s_gp.Group_ID 
				WHERE s_gp.Participant_ID = p.Participant_ID and s_gp.End_Date IS NULL and s_g.Group_Type_ID=4), -- Age
			1,
			IIF(ep.Event_Participant_ID IS NOT NULL AND ep.Participation_Status_ID IN (3, 4), 1, 0) AS 'Checkin',
			1,
			0
		from Group_Participants gp
		inner join event_groups eg on gp.Group_ID = eg.Group_ID
		inner join events e on e.Event_ID = eg.Event_ID
		inner join Groups g on gp.Group_ID = g.Group_ID
		inner join Participants p on p.Participant_ID = gp.Participant_ID
		inner join Contacts c on c.Contact_ID = p.Contact_ID
		LEFT JOIN Event_Participants ep on ep.Event_ID = e.Event_ID AND ep.Participant_ID = p.Participant_ID AND ep.Participation_Status_ID in (3, 4)
		where e.Event_Type_ID=243
		and e.Event_Start_Date >= @StartDate
		and e.Event_End_Date <= @EndDate
		and g.Group_Type_ID = 27
		and e.Congregation_ID = @CongregationId
		and exists (select * from groups s_g inner join event_groups s_eg on s_g.Group_ID = s_eg.Group_ID where s_eg.Event_ID = e.Event_ID
		and s_g.Group_Type_ID = 27)

	---- non RSVP'ed children
	INSERT INTO @ChildcareSummary SELECT DISTINCT
			e.Event_ID, -- Event_ID
			(select top(1) s_g.Group_Name from groups s_g inner join event_groups s_eg on s_g.Group_ID = s_eg.Group_ID where s_eg.Event_ID =
				e.Event_ID and s_g.Group_Type_ID NOT IN (4, 27)), -- Group_Name   
			e.Event_Start_Date as EventDate, -- Event Date
			e.Event_Start_Date as StartTime, -- Start Time
			e.Event_End_Date as EndTime, -- End Time
			p.Participant_ID, -- Participant ID
			(SELECT TOP(1) Group_Name from Groups s_g inner join Group_Participants s_gp on s_g.Group_ID = s_gp.Group_ID 
				WHERE s_gp.Participant_ID = p.Participant_ID and s_gp.End_Date IS NULL and s_g.Group_Type_ID=4), -- Age 
			0,
			IIF(ep.Event_Participant_ID IS NOT NULL AND ep.Participation_Status_ID IN (3, 4), 1, 0) AS 'Checkin',
			0,
			1
		from Event_Participants ep 
		inner join Events e on ep.Event_ID = e.Event_ID
		inner join Event_Groups eg on eg.Event_ID = e.Event_ID
		inner join Groups g on g.Group_ID = eg.Group_ID
		inner join Participants p on p.Participant_ID = ep.Participant_ID
		inner join Contacts c on c.Contact_ID = p.Contact_ID
		where e.Event_Type_ID=243
		and e.Event_Start_Date >= @StartDate
		and e.Event_End_Date <= @EndDate
		and e.Congregation_ID = @CongregationId
		and ep.Participation_Status_ID IN (3, 4) -- KC codes
		-- don't pull back a participant if they are part of the 27 group on the event
		and not exists (select * from Group_Participants s_gp inner join Event_Groups s_eg on s_gp.Group_ID = 
			s_eg.Group_ID inner join groups s_g on s_eg.Group_ID = s_g.Group_ID 
				where s_g.Group_Type_ID = 27 and s_gp.Participant_ID = p.Participant_ID and s_eg.Event_ID = e.Event_Id)

		SELECT Group_Name,
			EventDate,
			StartTime, 
			EndTime,
			Age,
			sum(RSVP) AS 'Total RSVP',
			sum(Checkin) AS 'Total Check In',
			sum(RSVPOnline) AS 'Rsvp Online',
			sum(RSVPOverride) AS 'Rsvp Override'
		FROM @ChildcareSummary
		GROUP BY Group_Name, EventDate, StartTime, EndTime, Age
		ORDER BY EventDate,Group_name, Age
    END;

GO


