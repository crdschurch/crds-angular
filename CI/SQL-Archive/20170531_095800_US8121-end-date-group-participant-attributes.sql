USE [MinistryPlatform]
GO

CREATE INDEX IX_Group_Participant_Attributes__GroupParticipantID ON Group_Participant_Attributes(Group_Participant_ID);
GO

CREATE TRIGGER crds_tr_End_Date_Group_Participant_Attributes
	ON Group_Participants
	AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

	-- When Group_Participants.End_Date is modified, apply the same change to Group_Participant_Attributes.End_Date
	IF UPDATE(End_Date)
	BEGIN
		UPDATE gpa
		SET gpa.End_Date = i.End_Date
		FROM
			INSERTED i
			INNER JOIN Group_Participant_Attributes gpa ON gpa.Group_Participant_ID = i.Group_Participant_ID
		;
	END
END
GO
