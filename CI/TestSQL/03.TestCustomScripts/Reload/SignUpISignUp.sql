DECLARE @eventTypeID as int
DECLARE @opportunityID as int

SELECT @eventTypeID =  Event_Type_ID, @opportunityID = Opportunity_ID 
FROM Opportunities 
WHERE Opportunity_Title = '(t+auto) Hey Serve UI'

DECLARE @groupID as int
DECLARE @participantID as int
SET @groupID = (SELECT Group_ID FROM Groups WHERE Group_Name = '(t+auto) Verify UI')
SET @participantID = (SELECT Participant_ID FROM Participants 
	WHERE Contact_ID = (SELECT TOP 1 Contact_ID 
	FROM Contacts WHERE Email_Address = 'mpcrds+auto+ISignUp@gmail.com' ))

SELECT Event_ID
INTO #eventTable  
FROM Events
WHERE Event_Type_ID = @eventTypeID AND Event_Start_Date > DATEADD(year,-1,GETDATE())

DECLARE @ID as int
WHILE EXISTS(SELECT * FROM #eventTable)
BEGIN
	SELECT TOP 1 @ID = Event_ID FROM #eventTable

	INSERT INTO Responses (Response_Date, Opportunity_ID, Participant_ID, Response_Result_ID, Domain_ID, Event_ID, Closed)
	VALUES (GETDATE(), @opportunityID, @participantID, 1, 1, @ID, 0)

	INSERT INTO Event_Participants (Event_ID, Participant_ID, Participation_Status_ID, Group_ID, Domain_ID)
	VALUES (@ID, @participantID, 2, @groupID, 1)
		
	DELETE FROM #eventTable WHERE Event_ID = @ID	
END

DROP TABLE #eventTable 