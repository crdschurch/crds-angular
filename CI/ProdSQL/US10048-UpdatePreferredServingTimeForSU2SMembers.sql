USE [MinistryPlatform]
GO

Begin Transaction

-- Get the list of all Group Participant's that have served in the last 2 years,
-- are still active, and do not have a preferrred serving time.
DECLARE @ServeData TABLE (
    Participant_ID INT,
    Display_Name NVARCHAR(75),
    Group_Participant_ID INT,
	Group_ID INT,
    Group_Name NVARCHAR(75),
    Email_Address NVARCHAR(255),
    Response_ID INT,
    rn INT,
	Preferred_Serve_Time_ID INT
);
INSERT INTO @ServeData
SELECT 
    p.Participant_ID,
    c.Display_Name,
    gp.Group_Participant_ID,
	g.Group_ID,
    g.Group_Name,
    c.Email_Address,
    r.Response_ID,
    rn = ROW_NUMBER() OVER (PARTITION BY p.Participant_ID, g.Group_ID, g.Congregation_ID ORDER BY r.Response_Date DESC),
	NULL
FROM participants p 
INNER JOIN responses r ON p.Participant_ID = r.Participant_ID 
INNER JOIN Contacts c ON c.Contact_ID = p.Contact_ID 
INNER JOIN Group_Participants gp on gp.Participant_ID = p.Participant_ID
INNER JOIN Groups g ON g.Group_ID = gp.Group_ID
WHERE g.Group_Type_ID = 9 -- Serving Team Group Type
AND r.Response_Date > DATEADD(MONTH, -24, GETDATE()) -- Response was in the last 2 years
AND r.Response_Result_ID = 1 -- Positive Response
AND p.Participant_End_Date IS NULL -- Paricipant record is still active
AND (gp.End_Date IS NULL OR gp.End_Date > GETDATE()) -- Group participant is still active
AND gp.Preferred_Serving_Time_ID IS NULL -- Does not have a current preferred serving time
--AND p.Participant_ID in (7510185, 7544723)
GROUP BY p.Participant_ID, 
    c.Display_Name,
    r.Response_Date,
    r.Response_ID, 
    g.Congregation_ID,
    g.Group_ID,
    g.Group_Name,
    gp.Group_Participant_ID,
    c.Email_Address

------------------------------
-- Update The Group Participants with their preferred serving time
DECLARE @EventTypes TABLE (
	Num_Times_Of_Serve_Time INT,
	Preferred_Serving_Time_Id INT,
	Congregation_ID INT
);

DECLARE @ParticipantId INT
DECLARE @GroupId INT
DECLARE @GroupParticipantId INT
DECLARE @PreferredServingTimeID INT
DECLARE @CongregationId INT

DECLARE participant_cursor CURSOR FOR SELECT Participant_ID, Group_ID, Group_Participant_ID FROM @ServeData WHERE rn = 1
OPEN participant_cursor
FETCH NEXT FROM participant_cursor INTO @ParticipantId, @GroupId, @GroupParticipantId
WHILE @@FETCH_STATUS = 0
BEGIN
	INSERT INTO @EventTypes
	SELECT COUNT(pst.Preferred_Serve_Time), pst.Preferred_Serving_Time_Id, e.Congregation_ID
	FROM Event_Participants ep
	INNER JOIN Events e ON ep.Event_ID = e.Event_ID
	INNER JOIN cr_Preferred_Serve_Time pst ON pst.Congregation_ID = e.Congregation_ID AND pst.Preferred_Serve_Time LIKE '%'+ FORMAT(e.Event_Start_Date, 'h:mm') +'%'
	WHERE ep.Participant_ID = @ParticipantId AND ep.Group_ID = @GroupId AND
	e.Event_Type_ID IN (94,95,96,97,98,99,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,
						119,121,122,123,124,125,126,127,128,129,133,134,135,136,137,140,141,171,172,173,174,175,176,177,
						179,180,236,237,238,239,240,241,242,245,246,247,248,249,250,251,252,255,270,271,272,273,274,275,276,277,
						349,364,368,380,381,382,383,384,385)

	GROUP BY pst.Preferred_Serving_Time_Id, e.Congregation_ID

    -- get the event information while filtering only on service event types]
	select TOP(1) @CongregationId = Congregation_ID, @PreferredServingTimeID = Preferred_Serving_Time_Id
	FROM @EventTypes
	ORDER BY Num_Times_Of_Serve_Time DESC
	
	IF @PreferredServingTimeID IS NOT NULL
	BEGIN
		UPDATE Group_Participants 
		SET Preferred_Serving_Time_ID = @PreferredServingTimeID
		WHERE Participant_ID = @ParticipantId AND Group_ID = @GroupId

		UPDATE @ServeData
		SET Preferred_Serve_Time_ID = @PreferredServingTimeID
		WHERE Participant_ID = @ParticipantId AND
			Group_ID = @GroupId AND
			Group_Participant_ID = @GroupParticipantId AND rn = 1
	END
		
	DELETE From @EventTypes
	SELECT @PreferredServingTimeID = NULL, @ParticipantId = NULL, @GroupId = NULL, @GroupParticipantId = NULL
    FETCH NEXT FROM participant_cursor INTO @ParticipantId, @GroupId, @GroupParticipantId
END
CLOSE participant_cursor
DEALLOCATE participant_cursor

SELECT s.*, ps.Preferred_Serve_Time
FROM @ServeData as s
INNER JOIN cr_Preferred_Serve_Time ps ON ps.Preferred_Serving_Time_ID = s.Preferred_Serve_Time_ID
WHERE s.rn = 1 AND s.Preferred_Serve_Time_ID IS NOT NULL

ROLLBACK