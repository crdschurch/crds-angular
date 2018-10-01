USE [MinistryPlatform]
GO

-- One-time data cleanup to make Group_Participant_Attributes.End_Date match Group_Participants.End_Date
-- This is part of US8121
UPDATE gpa
SET gpa.End_Date = gp.End_Date
FROM
	Group_Participants gp
	INNER JOIN Group_Participant_Attributes gpa ON gpa.Group_Participant_ID = gp.Group_Participant_ID
WHERE
	COALESCE(gp.End_Date, '1900-01-01') <> COALESCE(gpa.End_Date, '1900-01-01')
;
