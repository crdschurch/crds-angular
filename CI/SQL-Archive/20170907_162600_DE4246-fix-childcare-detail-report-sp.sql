USE [MinistryPlatform]
GO

ALTER PROCEDURE [dbo].[report_CRDS_Childcare_Detail]
	@StartDate DATETIME,
	@EndDate DATETIME,
	@CongregationId INT
AS
BEGIN
	SET NOCOUNT ON;

	SET @StartDate = DATEADD(day, DATEDIFF(day, 0, @StartDate), '00:00:00');
	SET @EndDate = DATEADD(day, DATEDIFF(day, 0, @EndDate), '23:59:00');

	DECLARE @AGE_GROUP_TYPE_ID int = 4;
	DECLARE @ChildcareGroupTypeId INT = 27;
	DECLARE @ChildcareEventTypeId INT = 243;
	DECLARE @AttendedId INT = 3;
	DECLARE @ConfirmedId INT = 4;

	DECLARE @ChildcareDetail TABLE
	(
		EventId int,
		Group_Name varchar(255),
		EventDate datetime,
		StartTime datetime,
		EndTime datetime,
		ParticipantId int,
		GroupMemberName varchar(255),
		ChildName varchar(255),
		Age nvarchar(200),
		Date_Of_Birth datetime,
		GroupParticipantStartDate datetime,
		Checkin nvarchar(6),
		GradeGroup nvarchar(255),
		RSVPOnline nvarchar(6),
		RSVPOverride nvarchar(6)
	)

	-- first case is children that had an rsvp
	INSERT INTO @ChildcareDetail SELECT
		e.Event_ID,
		(
			SELECT top(1) s_g.Group_Name
			FROM groups s_g
			INNER JOIN event_groups s_eg ON s_g.Group_ID = s_eg.Group_ID
			INNER JOIN Group_Participants s_gp ON s_g.Group_ID = s_gp.Group_ID
			WHERE s_gp.Group_Participant_ID = gp.Enrolled_By
		),
		e.Event_Start_Date AS EventDate,
		e.Event_Start_Date AS StartTime,
		e.Event_End_Date AS EndTime,
		p.Participant_ID,
		(
			SELECT top(1) Display_Name
			FROM Group_Participants s_gp
			INNER JOIN Participants s_p ON s_gp.Participant_ID = s_p.Participant_ID
			INNER JOIN Contacts s_c ON s_p.Contact_ID = s_c.Contact_ID
			WHERE s_gp.Group_Participant_ID = gp.Enrolled_By
		),
		c.Display_name AS 'ChildName',
		c.__Age AS Age,
		c.Date_of_Birth,
		gp.Start_Date AS GroupParticipantStartDate,
		IIF(ep.Event_Participant_ID IS NOT NULL AND ep.Participation_Status_ID IN (@AttendedId, @ConfirmedId), 'Yes', 'No') AS 'Checkin',
		(
			SELECT TOP(1) Group_Name
			FROM Groups s_g
			INNER JOIN Group_Participants s_gp ON s_g.Group_ID = s_gp.Group_ID
			WHERE s_gp.Participant_ID = p.Participant_ID
				AND s_gp.End_Date IS NULL AND s_g.Group_Type_ID = @AGE_GROUP_TYPE_ID
		),
		-- these magic numbers are just setting the RSVP-specific fields
		'Yes',
		'No'
	FROM Group_Participants gp
		INNER JOIN event_groups eg ON gp.Group_ID = eg.Group_ID
		INNER JOIN events e ON e.Event_ID = eg.Event_ID
		INNER JOIN Groups g ON gp.Group_ID = g.Group_ID
		INNER JOIN Participants p ON p.Participant_ID = gp.Participant_ID
		INNER JOIN Contacts c ON c.Contact_ID = p.Contact_ID
		LEFT JOIN Event_Participants ep ON
		(
			ep.Event_ID = e.Event_ID
			AND ep.Participant_ID = p.Participant_ID
			AND ep.Participation_Status_ID IN (@AttendedId, @ConfirmedId)
		)
	WHERE e.Event_Type_ID = @ChildcareEventTypeId
		AND e.Event_Start_Date >= @StartDate
		AND e.Event_End_Date <= @EndDate
		AND (gp.End_Date IS NULL OR gp.End_Date > e.Event_End_Date)
		AND g.Group_Type_ID = @ChildcareGroupTypeId
		AND e.Congregation_ID = @CongregationId
		AND EXISTS (
			SELECT 1
			FROM groups s_g
			INNER JOIN event_groups s_eg ON s_g.Group_ID = s_eg.Group_ID
			WHERE s_eg.Event_ID = e.Event_ID
			AND s_g.Group_Type_ID = @ChildcareGroupTypeId
		)

	-- non RSVP'ed children
	INSERT INTO @ChildcareDetail SELECT DISTINCT
		e.Event_ID,
		(
			SELECT top(1) s_g.Group_Name
			FROM groups s_g
			INNER JOIN event_groups s_eg ON s_g.Group_ID = s_eg.Group_ID
			WHERE s_eg.Event_ID = e.Event_ID
				AND s_g.Group_Type_ID NOT IN (@AGE_GROUP_TYPE_ID, @ChildcareGroupTypeId)
		), --g.Group_Name,
		e.Event_Start_Date AS EventDate,
		e.Event_Start_Date AS StartTime,
		e.Event_End_Date AS EndTime,
		p.Participant_ID,
		(
			SELECT top(1) Household_Name
			FROM Households
			WHERE household_id = ep.Checkin_Household_ID
		) AS 'Group Member Name', --parentscontact.Display_Name as 'GroupMemberName',
		c.Display_name AS 'ChildName',
		c.__Age AS Age,
		c.Date_of_Birth,
		ep.Time_In AS GroupParticipantStartDate,
		IIF(ep.Event_Participant_ID IS NOT NULL AND ep.Participation_Status_ID IN (@AttendedId, @ConfirmedId), 'Yes', 'No') AS 'Checkin',
		--NULL,
		(
			SELECT TOP(1) Group_Name
			FROM Groups s_g
			INNER JOIN Group_Participants s_gp ON s_g.Group_ID = s_gp.Group_ID
			WHERE s_gp.Participant_ID = p.Participant_ID
				AND s_gp.End_Date IS NULL
				AND s_g.Group_Type_ID = @AGE_GROUP_TYPE_ID
		),
		-- these magic numbers are just setting the RSVP-specific fields
		'No',
		'Yes'
	FROM Event_Participants ep
		INNER JOIN Events e ON ep.Event_ID = e.Event_ID
		INNER JOIN Event_Groups eg ON eg.Event_ID = e.Event_ID
		INNER JOIN Groups g ON g.Group_ID = eg.Group_ID
		INNER JOIN Participants p ON p.Participant_ID = ep.Participant_ID
		INNER JOIN Contacts c ON c.Contact_ID = p.Contact_ID
	WHERE e.Event_Type_ID = @ChildcareEventTypeId
		AND e.Event_Start_Date >= @StartDate
		AND e.Event_End_Date <= @EndDate
		AND e.Congregation_ID = @CongregationId
		AND ep.Participation_Status_ID IN (@AttendedId, @ConfirmedId) -- KC codes
		-- don't pull back a participant if they are part of the @ChildcareGroupTypeId (27) group on the event
		AND NOT EXISTS (
			SELECT 1
			FROM Group_Participants s_gp
			INNER JOIN Event_Groups s_eg ON s_gp.Group_ID = s_eg.Group_ID
			INNER JOIN groups s_g ON s_eg.Group_ID = s_g.Group_ID
			WHERE s_g.Group_Type_ID = @ChildcareGroupTypeId
				AND s_gp.Participant_ID = p.Participant_ID
				AND s_eg.Event_ID = e.Event_Id
		)

	SELECT * FROM @ChildcareDetail ORDER BY EventDate, Group_Name, EndTime
END