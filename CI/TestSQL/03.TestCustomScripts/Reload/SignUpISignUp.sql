
DECLARE @eventTypeID as int
DECLARE @opportunityID as int
DECLARE @groupParticipant as int

SELECT @eventTypeID =  Event_Type_ID, @opportunityID = Opportunity_ID 
FROM Opportunities 
WHERE Opportunity_Title = '(t+auto) Hey Serve UI'

DECLARE @groupID as int
DECLARE @participantID as int
SET @groupID = (SELECT Group_ID FROM Groups WHERE Group_Name = '(t+auto) Verify UI')
SET @participantID = (SELECT Participant_ID FROM Participants 
	WHERE Contact_ID = (SELECT TOP 1 Contact_ID 
	FROM Contacts WHERE Email_Address = 'mpcrds+auto+ISignUp@gmail.com' ))
SET @groupParticipant = (SELECT Group_Participant_ID 
	FROM Group_Participants 
	WHERE Group_ID = @groupID AND Participant_ID = @participantID)

DECLARE @eventIDs as table
(
	eventId int
)

SELECT Event_ID
INTO eventIds  
FROM Events
WHERE Event_Type_ID = @eventTypeID AND Event_Start_Date > DATEADD(year,-1,GETDATE())

DECLARE @eventId as int
WHILE EXISTS(SELECT * FROM eventIds)
BEGIN
	SELECT TOP 1 @eventId = eventId FROM eventIDs

	INSERT INTO Responses (Response_Date, Opportunity_ID, Participant_ID, Response_Result_ID, Domain_ID, Event_ID)
	VALUES (GETDATE(), @opportunityID, @participantID, 1, 1, @eventId)

	INSERT INTO Event_Participants (Event_ID, Participant_ID, Participation_Status_ID, Group_ID, Domain_ID)
	VALUES (@eventId, @participantID, 2, @groupID, 1)
		
	DELETE FROM eventIds WHERE eventId = @eventId	
END