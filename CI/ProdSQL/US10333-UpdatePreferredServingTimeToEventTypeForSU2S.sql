USE [MinistryPlatform]
GO

------------------------------------------------------------------------------------------------------
------ Add New Column For Preferred Serve Event Type ID ----------------------------------------------
------------------------------------------------------------------------------------------------------
IF COL_LENGTH('Group_Participants', 'Preferred_Serving_Event_Type_ID') IS NULL
BEGIN
	ALTER TABLE [dbo].[Group_Participants]
	ADD Preferred_Serving_Event_Type_ID INT;

	ALTER TABLE [dbo].[Group_Participants]  
	WITH CHECK ADD  
	CONSTRAINT [FK_Group_Participant_Event_Type] 
	FOREIGN KEY(Preferred_Serving_Event_Type_ID)
	REFERENCES [dbo].[Event_Types] ([Event_Type_ID])
END

------------------------------------------------------------------------------------------------------
------ Update Preferred Serve Time to help scripting -------------------------------------------------
------------------------------------------------------------------------------------------------------

UPDATE cr_Preferred_Serve_Time 
SET Preferred_Serve_Time = 'Saturday at 5:30'
WHERE Preferred_Serving_Time_ID = 56

------------------------------------------------------------------------------------------------------
------ Script to update records with Preferred Serving Time to also have a Preferred Event Type ------
------------------------------------------------------------------------------------------------------
Begin Transaction

-- Get the list of all Group Participant's that have a preferred serving time
DECLARE @ServeData TABLE (
    Participant_ID INT,
    Display_Name NVARCHAR(75),
    Group_Participant_ID INT,
	Group_ID INT,
    Group_Name NVARCHAR(75),
    Email_Address NVARCHAR(255),
	Preferred_Serving_Time_ID INT,
    Preferred_Serve_Time NVARCHAR(255),
	Preferred_Event_Type_ID INT
);
INSERT INTO @ServeData
SELECT DISTINCT
    p.Participant_ID,
    c.Display_Name,
    gp.Group_Participant_ID,
	g.Group_ID,
    g.Group_Name,
    c.Email_Address,
	gp.Preferred_Serving_Time_ID,
	pst.Preferred_Serve_Time,
	NULL
FROM Group_Participants gp
INNER JOIN Participants p ON gp.Participant_ID = p.Participant_ID
INNER JOIN Contacts c ON c.Contact_ID = p.Contact_ID 
INNER JOIN Groups g ON g.Group_ID = gp.Group_ID
INNER JOIN cr_Preferred_Serve_Time pst ON pst.Preferred_Serving_Time_ID = gp.Preferred_Serving_Time_ID
WHERE (p.Participant_End_Date IS NULL OR p.Participant_End_Date > GETDATE()) -- Paricipant record is still active
AND (gp.End_Date IS NULL OR gp.End_Date > GETDATE()) -- Group participant is still active
AND gp.Preferred_Serving_Time_ID IS NOT NULL -- Has a current preferred serving time
AND gp.Preferred_Serving_Time_ID <> 5 -- Ignore N/A Preferred Serving Times


DECLARE @GroupParticipantId INT
DECLARE @PreferredEventTypeID INT

DECLARE participant_cursor CURSOR FOR SELECT Group_Participant_ID FROM @ServeData
OPEN participant_cursor
FETCH NEXT FROM participant_cursor INTO @GroupParticipantId
WHILE @@FETCH_STATUS = 0
BEGIN

	SELECT DISTINCT @PreferredEventTypeID = et.Event_Type_ID
	FROM Group_Participants gp
	INNER JOIN cr_Preferred_Serve_Time pst ON gp.Preferred_Serving_Time_ID = pst.Preferred_Serving_Time_ID
	INNER JOIN Events e ON e.Congregation_ID = pst.Congregation_ID AND pst.Preferred_Serve_Time LIKE '%'+ FORMAT(e.Event_Start_Date, 'h:mm') +'%'
	INNER JOIN Event_Types et ON et.Event_Type_ID = e.Event_Type_ID
	WHERE gp.Group_Participant_ID = @GroupParticipantId AND
	et.Event_Type LIKE '%'+ pst.Preferred_Serve_Time +'%' AND
	e.Event_Type_ID IN (94, 95, 96, 97, 101, 111, 121, 129, 133, 134, 135, 136, 236, 237, 238, 239, 240,
		241, 242, 245, 246, 247, 248, 249, 349, 364, 368, 380, 381, 382, 383, 384, 385)
	
	IF @PreferredEventTypeID IS NOT NULL
	BEGIN
		UPDATE Group_Participants 
		SET Preferred_Serving_Event_Type_ID = @PreferredEventTypeID
		WHERE Group_Participant_ID = @GroupParticipantId

		UPDATE @ServeData
		SET Preferred_Event_Type_ID = @PreferredEventTypeID
		WHERE Group_Participant_ID = @GroupParticipantId
	END

	SELECT @PreferredEventTypeID = NULL
    FETCH NEXT FROM participant_cursor INTO @GroupParticipantId
END
CLOSE participant_cursor
DEALLOCATE participant_cursor

-- Display 
SELECT s.*, et.Event_Type FROM @ServeData s
INNER JOIN Event_Types et ON et.Event_Type_ID = s.Preferred_Event_Type_ID
WHERE Preferred_Event_Type_ID IS NOT NULL

Rollback