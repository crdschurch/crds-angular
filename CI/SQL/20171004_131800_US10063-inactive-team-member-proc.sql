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
		rd DATETIME,
		pid INT,
		dn NVARCHAR(200),
		gn NVARCHAR(200),
		em NVARCHAR(200),
		mp NVARCHAR(50),
		rn INT
	);

	INSERT INTO @ServeData

	SELECT DISTINCT
	r.Response_Date AS rd, 
	p.Participant_ID AS pid,
	c.First_Name + ' ' + c.Last_Name AS dn,	
	g.Group_Name AS gn,
	c.Email_Address AS em,
	c.Mobile_Phone as mp,
	rn = ROW_NUMBER() OVER (PARTITION BY p.Participant_ID, g.Group_Name
                                    ORDER BY r.Response_Date DESC)
	FROM participants p 
	INNER JOIN responses r ON p.Participant_ID = r.Participant_ID 
	INNER JOIN Contacts c ON c.Contact_ID = p.Contact_ID 
	INNER JOIN Group_Participants gp on gp.Participant_ID = p.Participant_ID
	INNER JOIN Groups g ON g.Group_ID = gp.Group_ID
	WHERE g.Group_Type_ID = 9 -- Serving Team Group Type
	AND r.Response_Date < DATEADD(MONTH, -3, GETDATE())
	AND g.Group_Id IN (SELECT Item FROM dbo.dp_Split(@GroupID, ','))
	AND gp.End_Date IS NOT NULL
	GROUP BY p.Participant_ID, c.First_Name, c.Last_Name, r.Response_Date, g.Group_Name, c.Email_Address, c.Mobile_Phone

	-- if contact doesn't have a mobile phone, default to their household phone
	UPDATE @ServeData SET mp = (SELECT TOP(1) Home_Phone FROM
		Participants p INNER JOIN Contacts c ON p.Contact_ID = c.Contact_ID
		INNER JOIN Households h ON h.Household_ID = c.Household_ID) 
	WHERE mp IS NULL 

	SELECT pid as "Participant ID", dn as "Name", gn as "Team", em as "Email", rd as "Last Serve Date", mp as "Mobile Phone" FROM @ServeData WHERE rn=1 ORDER BY rd DESC, gn, dn;
END