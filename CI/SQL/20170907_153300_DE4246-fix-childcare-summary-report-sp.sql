USE [MinistryPlatform]
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
		RSVP int,
		Checkin int,
		RSVPOnline int,
		RSVPOverride int,
		ParticipantAge int
	)

	-- first case is children that had an rsvp
	INSERT INTO @ChildcareSummary SELECT
		e.Event_ID, -- Event_ID
		(
			select top(1) s_g.Group_Name
			from groups s_g
			inner join event_groups s_eg on s_g.Group_ID = s_eg.Group_ID
			inner join Group_Participants s_gp on s_g.Group_ID = s_gp.Group_ID
			where s_gp.Group_Participant_ID = gp.Enrolled_By
		), -- Group_Name
		e.Event_Start_Date as EventDate, -- EventDate
		e.Event_Start_Date as StartTime, -- StartTime
		e.Event_End_Date as EndTime, -- EndTime
		p.Participant_ID, --- ParticipantID
		1,
		IIF(ep.Event_Participant_ID IS NOT NULL AND ep.Participation_Status_ID IN (3, 4), 1, 0) AS 'Checkin',
		1,
		0,
		--c.Date_of_Birth
		(SELECT DATEDIFF(YY, c.Date_of_Birth, GETDATE()) - CASE WHEN RIGHT(CONVERT(VARCHAR(6), GETDATE(), 12), 4) >= 
			RIGHT(CONVERT(VARCHAR(6), c.Date_of_Birth, 12), 4) THEN 0 ELSE 1 END) AS ParticipantAge
	FROM Group_Participants gp
		inner join event_groups eg on gp.Group_ID = eg.Group_ID
		inner join events e on e.Event_ID = eg.Event_ID
		inner join Groups g on gp.Group_ID = g.Group_ID
		inner join Participants p on p.Participant_ID = gp.Participant_ID
		inner join Contacts c on c.Contact_ID = p.Contact_ID
		LEFT JOIN Event_Participants ep on ep.Event_ID = e.Event_ID AND ep.Participant_ID = p.Participant_ID AND ep.Participation_Status_ID in (3, 4)
	WHERE e.Event_Type_ID = 243
		and e.Event_Start_Date >= @StartDate
		and e.Event_End_Date <= @EndDate
		AND (gp.End_Date IS NULL OR gp.End_Date > e.Event_End_Date)
		and g.Group_Type_ID = 27
		and e.Congregation_ID = @CongregationId
		and exists (
			select 1
			from groups s_g
			inner join event_groups s_eg on s_g.Group_ID = s_eg.Group_ID
			where s_eg.Event_ID = e.Event_ID and s_g.Group_Type_ID = 27
		)

	---- non RSVP'ed children
	INSERT INTO @ChildcareSummary SELECT DISTINCT
		e.Event_ID, -- Event_ID
		(select top(1) s_g.Group_Name from groups s_g inner join event_groups s_eg on s_g.Group_ID = s_eg.Group_ID where s_eg.Event_ID =
			e.Event_ID and s_g.Group_Type_ID NOT IN (4, 27)), -- Group_Name   
		e.Event_Start_Date as EventDate, -- Event Date
		e.Event_Start_Date as StartTime, -- Start Time
		e.Event_End_Date as EndTime, -- End Time
		p.Participant_ID, -- Participant ID
		0,
		IIF(ep.Event_Participant_ID IS NOT NULL AND ep.Participation_Status_ID IN (3, 4), 1, 0) AS 'Checkin',
		0,
		1,
		(SELECT DATEDIFF(YY, c.Date_of_Birth, GETDATE()) - CASE WHEN RIGHT(CONVERT(VARCHAR(6), GETDATE(), 12), 4) >= 
			RIGHT(CONVERT(VARCHAR(6), c.Date_of_Birth, 12), 4) THEN 0 ELSE 1 END) AS ParticipantAge
	FROM Event_Participants ep 
		inner join Events e on ep.Event_ID = e.Event_ID
		inner join Event_Groups eg on eg.Event_ID = e.Event_ID
		inner join Groups g on g.Group_ID = eg.Group_ID
		inner join Participants p on p.Participant_ID = ep.Participant_ID
		inner join Contacts c on c.Contact_ID = p.Contact_ID
	WHERE e.Event_Type_ID=243
		and e.Event_Start_Date >= @StartDate
		and e.Event_End_Date <= @EndDate
		and e.Congregation_ID = @CongregationId
		and ep.Participation_Status_ID IN (3, 4) -- KC codes
		-- don't pull back a participant if they are part of the 27 group on the event
		and not exists (
			select 1
			from Group_Participants s_gp
			inner join Event_Groups s_eg on s_gp.Group_ID = s_eg.Group_ID
			inner join groups s_g on s_eg.Group_ID = s_g.Group_ID 
			where s_g.Group_Type_ID = 27 and s_gp.Participant_ID = p.Participant_ID and s_eg.Event_ID = e.Event_Id
		)

	SELECT
		Event_ID, 
		Group_Name,
		EventDate,
		StartTime, 
		EndTime,
		--Age,
		sum(RSVP) AS 'Total RSVP',
		sum(Checkin) AS 'Total Check In',
		sum(RSVPOnline) AS 'Rsvp Online',
		sum(RSVPOverride) AS 'Rsvp Override',
		ParticipantAge
	FROM @ChildcareSummary
	GROUP BY Event_ID, Group_Name, EventDate, StartTime, EndTime, ParticipantAge
	ORDER BY EventDate,Group_name,ParticipantAge
END
GO
