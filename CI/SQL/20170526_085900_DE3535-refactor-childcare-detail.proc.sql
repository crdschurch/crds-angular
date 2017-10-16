USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Childcare_Detail]    Script Date: 6/6/2017 7:42:38 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ===============================================================
-- Author: John Cleaver	
-- Create date: 6/6/2017
-- Description:	Refactor of report_CRDS_Childcare_Detail proc
-- ===============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[report_CRDS_Childcare_Detail]') AND type in (N'P', N'PC'))
BEGIN
	EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[report_CRDS_Childcare_Detail] AS' 
END
GO

ALTER PROCEDURE [dbo].[report_CRDS_Childcare_Detail]

      @StartDate DATETIME,
	  @EndDate DATETIME,
	  @CongregationId INT
AS
    BEGIN

	SET NOCOUNT ON;

	SET @StartDate =  DATEADD(day, DATEDIFF(day, 0, @StartDate), '00:00:00');
	SET @EndDate =  DATEADD(day, DATEDIFF(day, 0, @EndDate), '23:59:00');

	DECLARE @AGE_GROUP_TYPE_ID int = 4;

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
	INSERT INTO @ChildcareDetail SELECT DISTINCT
			e.Event_ID,
			(select top(1) s_g.Group_Name from groups s_g inner join event_groups s_eg on s_g.Group_ID = s_eg.Group_ID inner join Group_Participants s_gp
				on s_g.Group_ID = s_gp.Group_ID where s_gp.Group_Participant_ID = gp.Enrolled_By),    
			e.Event_Start_Date as EventDate, 
			e.Event_Start_Date as StartTime, 
			e.Event_End_Date as EndTime, 
			p.Participant_ID,
			(select top(1) Display_Name from Group_Participants s_gp INNER JOIN Participants s_p ON s_gp.Participant_ID = s_p.Participant_ID
				INNER JOIN Contacts s_c ON s_p.Contact_ID = s_c.Contact_ID where s_gp.Group_Participant_ID = gp.Enrolled_By),
			c.Display_name as 'ChildName',
			c.__Age as Age, 
			c.Date_of_Birth,
			gp.Start_Date as GroupParticipantStartDate,
			IIF(ep.Event_Participant_ID IS NOT NULL AND ep.Participation_Status_ID IN (3, 4), 'Yes', 'No') AS 'Checkin',
			(SELECT TOP(1) Group_Name from Groups s_g inner join Group_Participants s_gp on s_g.Group_ID = s_gp.Group_ID 
				WHERE s_gp.Participant_ID = p.Participant_ID and s_gp.End_Date IS NULL and s_g.Group_Type_ID=4),
				-- these magic numbers are just setting the RSVP-specific fields
				'Yes',
				'No'
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

	-- non RSVP'ed children
	INSERT INTO @ChildcareDetail SELECT DISTINCT
			e.Event_ID,
			(select top(1) s_g.Group_Name from groups s_g inner join event_groups s_eg on s_g.Group_ID = s_eg.Group_ID where s_eg.Event_ID =
				e.Event_ID and s_g.Group_Type_ID NOT IN (4, 27)), --g.Group_Name,    
			e.Event_Start_Date as EventDate, 
			e.Event_Start_Date as StartTime, 
			e.Event_End_Date as EndTime, 
			p.Participant_ID,
			(select top(1) Household_Name from Households where household_id = ep.Checkin_Household_ID) as 'Group Member Name', --parentscontact.Display_Name as 'GroupMemberName',
			c.Display_name as 'ChildName',
			c.__Age as Age, 
			c.Date_of_Birth,
			ep.Time_In as GroupParticipantStartDate,
			IIF(ep.Event_Participant_ID IS NOT NULL AND ep.Participation_Status_ID IN (3, 4), 'Yes', 'No') AS 'Checkin',
			--NULL,
			(SELECT TOP(1) Group_Name from Groups s_g inner join Group_Participants s_gp on s_g.Group_ID = s_gp.Group_ID 
				WHERE s_gp.Participant_ID = p.Participant_ID and s_gp.End_Date IS NULL and s_g.Group_Type_ID=4),
				-- these magic numbers are just setting the RSVP-specific fields
				'No',
				'Yes'
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

	SELECT * FROM @ChildcareDetail ORDER BY EventDate, Group_Name, EndTime

END

GO


