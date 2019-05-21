USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[report_CRDS_InactiveServeTeamMembers]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[report_CRDS_InactiveServeTeamMembers] AS'
END
GO
-- =============================================
-- Author:      John Cleaver
-- Create date: 2017-10-04
-- Description:	Gets team members who have not served in the last 90 days
-- =============================================
ALTER PROCEDURE [dbo].report_CRDS_InactiveServeTeamMembers
	@GroupId VARCHAR(MAX)
AS
BEGIN

	DECLARE @ServeData TABLE (
		esd DATETIME,
		pid INT,
		dn NVARCHAR(200),
		gn NVARCHAR(200),
		gid INT,
		em NVARCHAR(200),
		mp NVARCHAR(50),
		rn INT
	);

	-- Get people who have served before, but have a 90+ day gap since the last time they served
	INSERT INTO @ServeData
	SELECT DISTINCT
	e.Event_Start_Date AS esd,
	p.Participant_ID AS pid,
	c.First_Name + ' ' + c.Last_Name AS dn,	
	g.Group_Name AS gn,
	g.Group_ID as gid,
	c.Email_Address AS em,
	c.Mobile_Phone as mp,
	rn = ROW_NUMBER() OVER (PARTITION BY p.Participant_ID, g.Group_Name
                                    ORDER BY e.Event_Start_date DESC)
	FROM participants p 
	INNER JOIN responses r ON p.Participant_ID = r.Participant_ID 
	INNER JOIN Contacts c ON c.Contact_ID = p.Contact_ID 
	INNER JOIN Group_Participants gp on gp.Participant_ID = p.Participant_ID
	INNER JOIN Groups g ON g.Group_ID = gp.Group_ID
	INNER JOIN Opportunities o ON o.Opportunity_ID = r.Opportunity_ID
	INNER JOIN Events e on e.Event_ID = r.Event_ID
	WHERE g.Group_Type_ID = 9 -- Serving Team Group Type
	AND g.Group_Id IN (SELECT Item FROM dbo.dp_Split(@GroupID, ','))
	AND (gp.End_Date IS NULL OR gp.End_Date > GETDATE())
	AND gp.Group_ID IN (SELECT Item FROM dbo.dp_Split(@GroupID, ','))
	AND o.Add_to_Group IN (SELECT Item FROM dbo.dp_Split(@GroupID, ','))
	AND r.Response_Result_ID = 1 -- 1 = "Placed"
	AND e.Event_Start_Date < GETDATE() -- look only at past signups
	GROUP BY p.Participant_ID, c.First_Name, c.Last_Name, e.Event_Start_Date, g.Group_Name, g.Group_ID, c.Email_Address, c.Mobile_Phone

	-- Get people who have signed up for the group more than 90 days ago, but have never served for that group
	INSERT INTO @ServeData
	SELECT DISTINCT
	NULL AS esd,
	p.Participant_ID AS pid,
	c.First_Name + ' ' + c.Last_Name AS dn,	
	g.Group_Name AS gn,
	g.Group_ID as gid,
	c.Email_Address AS em,
	c.Mobile_Phone as mp,
	1 as rn
	FROM participants p 
	INNER JOIN Contacts c ON c.Contact_ID = p.Contact_ID 
	INNER JOIN Group_Participants gp on gp.Participant_ID = p.Participant_ID
	INNER JOIN Groups g ON g.Group_ID = gp.Group_ID
	WHERE g.Group_Id IN (SELECT Item FROM dbo.dp_Split(@GroupID, ','))
	AND (gp.End_Date IS NULL OR gp.End_Date > GETDATE())
	AND gp.Group_ID IN (SELECT Item FROM dbo.dp_Split(@GroupID, ','))
	AND gp.Start_Date < DATEADD(MONTH, -3, GETDATE())
	-- avoid dupes based on a person having non-serving responses
	AND NOT EXISTS (SELECT * FROM @ServeData WHERE pid = gp.Participant_ID) 
	AND NOT EXISTS (SELECT * FROM 
		Responses s_r INNER JOIN Opportunities s_o ON s_r.Opportunity_ID = s_o.Opportunity_ID 
		WHERE s_r.Participant_ID = gp.Group_Participant_ID
		AND s_o.Add_to_Group = gp.Group_Participant_ID)
	GROUP BY p.Participant_ID, c.First_Name, c.Last_Name, g.Group_Name, g.Group_ID, c.Email_Address, c.Mobile_Phone

	-- if contact doesn't have a mobile phone, default to their household phone
	UPDATE @ServeData SET mp = (SELECT TOP(1) Home_Phone FROM
		Participants p INNER JOIN Contacts c ON p.Contact_ID = c.Contact_ID
		INNER JOIN Households h ON h.Household_ID = c.Household_ID) 
	WHERE mp IS NULL 

	SELECT pid as "Participant ID", dn as "Name", gn as "Team", gid as "Group ID", em as "Email", esd as "Last Serve Date", mp as "Mobile Phone" FROM @ServeData WHERE rn=1 AND (esd < DATEADD(MONTH, -3, GETDATE()) OR esd IS NULL) ORDER BY esd DESC, gn, dn;
END